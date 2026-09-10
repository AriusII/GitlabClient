using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.RateLimiting;
using GitLab.Client.Models.Responses;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Infrastructure.Http;

/// <summary>
///     Maps a non-success GitLab response onto the exception type that describes it. Deliberately one switch over
///     the status code: no reflection, no <c>Dictionary&lt;HttpStatusCode, Type&gt;</c>, no
///     <c>Activator.CreateInstance</c> - every construction site is statically visible to the trimmer and to
///     Native AOT.
/// </summary>
internal static class GitLabApiExceptionFactory
{
    /// <summary>
    ///     How much of a response body an exception is allowed to retain. Comfortably more than any GitLab
    ///     JSON error - the biggest documented ones run to a few hundred bytes - and small enough that the
    ///     failure mode this guards against costs nothing: a reverse proxy in front of a self-managed
    ///     instance answering a 502 with a multi-megabyte HTML page, held alive for as long as the exception
    ///     is, which for a logged-and-rethrown error can be the lifetime of the request pipeline.
    /// </summary>
    internal const int MaxRetainedResponseBodyLength = 8 * 1024;

    public static GitLabApiException Create(
        HttpResponseMessage response,
        HttpMethod requestMethod,
        Uri requestUri,
        string responseBody)
    {
        HttpStatusCode statusCode = response.StatusCode;
        Uri? uri = response.RequestMessage?.RequestUri ?? requestUri;

        // The transport supplies an already bounded body. Parse the prefix when it is a complete GitLab error
        // document; proxy error pages and deliberately truncated payloads still remain useful as raw text.
        GitLabErrorResponse? error = TryParseError(responseBody);
        string retainedBody = Truncate(responseBody);
        string message = BuildMessage(error, retainedBody, statusCode);

        return statusCode switch
        {
            HttpStatusCode.Unauthorized =>
                new GitLabAuthenticationException(statusCode, message, retainedBody, requestMethod, uri),
            HttpStatusCode.Forbidden =>
                new GitLabForbiddenException(statusCode, message, retainedBody, requestMethod, uri),
            HttpStatusCode.NotFound =>
                new GitLabNotFoundException(statusCode, message, retainedBody, requestMethod, uri),
            HttpStatusCode.Conflict =>
                new GitLabConflictException(statusCode, message, retainedBody, requestMethod, uri),
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
                CreateValidation(statusCode, message, retainedBody, requestMethod, uri, error),
            HttpStatusCode.TooManyRequests =>
                new GitLabRateLimitExceededException(
                    statusCode,
                    message,
                    retainedBody,
                    requestMethod,
                    uri,
                    ReadRetryAfter(response.Headers),
                    GitLabRateLimitSnapshot.FromHeaders(response.Headers)),
            _ when (int)statusCode >= 500 =>
                new GitLabServerException(statusCode, message, retainedBody, requestMethod, uri),
            _ => new GitLabApiException(statusCode, message, retainedBody, requestMethod, uri)
        };
    }

    /// <summary>
    ///     For a response that succeeded at the HTTP level but carried no body where one was required. Stays the
    ///     base type so existing <c>catch (GitLabApiException)</c> blocks keep covering it.
    /// </summary>
    public static GitLabApiException CreateForEmptyBody(
        HttpResponseMessage response,
        HttpMethod requestMethod,
        Uri requestUri)
    {
        return new GitLabApiException(
            response.StatusCode,
            "GitLab returned an empty response body.",
            null,
            requestMethod,
            response.RequestMessage?.RequestUri ?? requestUri);
    }

    private static GitLabValidationException CreateValidation(
        HttpStatusCode statusCode,
        string message,
        string responseBody,
        HttpMethod requestMethod,
        Uri? requestUri,
        GitLabErrorResponse? error)
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> errors =
            error?.ToValidationErrors() ?? ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;

        // A structured "message" object renders as raw JSON through ToDisplayMessage(); flatten it into
        // something a human can read, and leave plain-string messages exactly as GitLab sent them.
        string effectiveMessage = errors.Count == 0 ? message : Flatten(errors);

        return new GitLabValidationException(
            statusCode,
            effectiveMessage,
            responseBody,
            requestMethod,
            requestUri,
            errors);
    }

    private static string Flatten(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
    {
        StringBuilder builder = new(128);

        foreach (KeyValuePair<string, IReadOnlyList<string>> entry in errors)
        {
            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(entry.Key).Append(": ").Append(string.Join(", ", entry.Value));
        }

        return builder.ToString();
    }

    private static GitLabErrorResponse? TryParseError(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(responseBody, GitLabJsonContext.Default.GitLabErrorResponse);
        }
        catch (JsonException)
        {
            // Not the documented error shape - an HTML error page from a proxy, say. The raw body still
            // reaches the caller through GitLabApiException.ResponseBody.
            return null;
        }
    }

    private static string BuildMessage(GitLabErrorResponse? error, string responseBody, HttpStatusCode statusCode)
    {
        string? display = error?.ToDisplayMessage();

        if (!string.IsNullOrWhiteSpace(display))
        {
            return display;
        }

        // A bounded transport read can cut off a valid JSON document after its first property. Preserve the
        // overwhelmingly common {"message":"..."} diagnostic without retaining or parsing the rest of a
        // potentially huge proxy response. Structured validation objects still require a complete document
        // and intentionally fall back to the retained prefix when the server sent a truncated body.
        string? leadingMessage = TryReadLeadingStringMessage(responseBody);
        return !string.IsNullOrWhiteSpace(leadingMessage)
            ? leadingMessage
            : !string.IsNullOrWhiteSpace(responseBody)
                ? responseBody
                : string.Create(
                    CultureInfo.InvariantCulture,
                    $"GitLab returned {(int)statusCode} {statusCode} with no response body.");
    }

    private static string? TryReadLeadingStringMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(responseBody), false, default);
        bool isMessageProperty = false;

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    isMessageProperty = reader.ValueTextEquals("message"u8);
                    continue;
                }

                if (isMessageProperty && reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString();
                }

                isMessageProperty = false;
            }
        }
        catch (JsonException)
        {
            // The exact expected path: the response ended after the retained prefix. The already-read
            // message token, if any, was returned above.
        }

        return null;
    }

    /// <summary>
    ///     Caps what an exception keeps alive at <see cref="MaxRetainedResponseBodyLength" /> characters,
    ///     saying plainly how much was dropped so nobody debugs a truncated payload thinking it is the whole
    ///     one. Returns the original instance untouched in the overwhelmingly common case, so the normal
    ///     error path allocates nothing extra.
    /// </summary>
    private static string Truncate(string responseBody)
    {
        if (responseBody.Length <= MaxRetainedResponseBodyLength)
        {
            return responseBody;
        }

        // Never split a surrogate pair: the tail of the retained text would otherwise be a lone half that
        // renders as a replacement character in every log sink downstream.
        int keep = MaxRetainedResponseBodyLength;

        if (char.IsHighSurrogate(responseBody[keep - 1]))
        {
            keep--;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{responseBody.AsSpan(0, keep)}... [{responseBody.Length - keep} more characters elided]");
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseHeaders headers)
    {
        RetryConditionHeaderValue? retryAfter = headers.RetryAfter;

        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta is { } delta)
        {
            return delta;
        }

        if (retryAfter.Date is not { } date)
        {
            return null;
        }

        TimeSpan remaining = date - DateTimeOffset.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}