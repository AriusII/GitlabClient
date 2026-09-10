using System.Net;

using GitLab.Client.Infrastructure.RateLimiting;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     GitLab throttled the request (HTTP 429). The request itself was fine and
///     <see cref="GitLabApiException.IsTransient" /> is true. This exception only reports GitLab's guidance; it
///     never waits or throttles a caller.
/// </summary>
public sealed class GitLabRateLimitExceededException : GitLabApiException
{
    /// <summary>Initializes a new instance with a default message and no GitLab response context.</summary>
    public GitLabRateLimitExceededException()
    {
    }

    /// <summary>Initializes a new instance with the supplied message and no GitLab response context.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabRateLimitExceededException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabRateLimitExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance from a throttled GitLab response, carrying the throttling headers.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    /// <param name="retryAfter">The wait GitLab asked for, from the <c>Retry-After</c> header.</param>
    /// <param name="rateLimit">The <c>RateLimit-*</c> headers as they stood on the throttled response.</param>
    public GitLabRateLimitExceededException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri,
        TimeSpan? retryAfter,
        GitLabRateLimitSnapshot rateLimit)
        : base(statusCode, message, responseBody, requestMethod, requestUri)
    {
        RetryAfter = retryAfter;
        RateLimit = rateLimit;
    }

    /// <summary>
    ///     How long GitLab asked the caller to wait, from the <c>Retry-After</c> header. Handles both the
    ///     delta-seconds and the HTTP-date forms; null when the header was absent. In that case,
    ///     <see cref="GitLabRateLimitSnapshot.ResetTime" /> and <see cref="GitLabRateLimitSnapshot.ResetsAt" />
    ///     are absolute reset-time observations. <see cref="GitLabRateLimitSnapshot.RetryAfter" /> likewise
    ///     handles both legal <c>Retry-After</c> forms.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>The <c>RateLimit-*</c> headers as they stood on the throttled response.</summary>
    public GitLabRateLimitSnapshot RateLimit { get; }
}