using System.Globalization;
using System.Net;
using System.Text;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     Base type for every error the GitLab REST API reports as a non-success HTTP status code. Catch this to
///     handle any API failure; catch one of the derived types to handle a specific class of failure -
///     <see cref="GitLabNotFoundException" />, <see cref="GitLabValidationException" />,
///     <see cref="GitLabRateLimitExceededException" /> and friends.
/// </summary>
/// <remarks>
///     Transport-level failures are deliberately not wrapped: a DNS, TLS or socket failure still surfaces as
///     <see cref="HttpRequestException" />, and a cancelled or timed-out request still surfaces as
///     <see cref="TaskCanceledException" />, so cancellation and retry semantics stay intact.
/// </remarks>
public class GitLabApiException : Exception
{
    /// <summary>Initializes a new instance with a default message and no GitLab response context.</summary>
    public GitLabApiException()
    {
    }

    /// <summary>Initializes a new instance with the supplied message and no GitLab response context.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabApiException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance from a GitLab response, without the request context.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    public GitLabApiException(HttpStatusCode statusCode, string message, string? responseBody)
        : this(statusCode, message, responseBody, null, null)
    {
    }

    /// <summary>Initializes a new instance from a GitLab response, carrying the request that produced it.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    public GitLabApiException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RequestMethod = requestMethod;
        RequestUri = requestUri;
    }

    /// <summary>The HTTP status code GitLab responded with.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>The raw response body, for callers that need more detail than <see cref="Exception.Message" />.</summary>
    public string? ResponseBody { get; }

    /// <summary>
    ///     The verb of the request that failed. Null only when the exception was built through one of the three
    ///     standard constructors rather than by the client's own error mapping.
    /// </summary>
    public HttpMethod? RequestMethod { get; }

    /// <summary>
    ///     The URI of the request that failed. Null only when the exception was built through one of the three
    ///     standard constructors rather than by the client's own error mapping.
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     True when the same request is worth retrying unchanged: HTTP 408, 429 and any 5xx. Lets a caller write
    ///     one retry policy without switching on <see cref="StatusCode" /> or on the exception type.
    /// </summary>
    public bool IsTransient =>
        StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
        || (int)StatusCode >= 500;

    /// <summary>
    ///     Appends the failing request and status to the standard exception text. Deliberately kept out of
    ///     <see cref="Exception.Message" />, which stays exactly the message GitLab sent.
    /// </summary>
    /// <returns>The base exception text, followed by the request line when this exception carries one.</returns>
    public override string ToString()
    {
        string baseText = base.ToString();

        if (RequestUri is null && StatusCode == default)
        {
            return baseText;
        }

        StringBuilder builder = new(baseText, baseText.Length + 96);
        builder.Append(Environment.NewLine);
        builder.Append("   GitLab request: ");
        builder.Append(RequestMethod?.Method ?? "(unknown method)");
        builder.Append(' ');
        builder.Append(RequestUri?.ToString() ?? "(unknown uri)");
        builder.Append(" -> ");
        builder.Append(((int)StatusCode).ToString(CultureInfo.InvariantCulture));
        builder.Append(' ');
        builder.Append(StatusCode.ToString());
        return builder.ToString();
    }
}