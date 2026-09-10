using System.Net;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     GitLab rejected the credentials on the request (HTTP 401). The token is missing, malformed, expired or
///     revoked. Re-authenticating is the only useful response - retrying the same request is not.
/// </summary>
public sealed class GitLabAuthenticationException : GitLabApiException
{
    /// <summary>Initializes a new instance with a default message and no GitLab response context.</summary>
    public GitLabAuthenticationException()
    {
    }

    /// <summary>Initializes a new instance with the supplied message and no GitLab response context.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance from a GitLab response, carrying the request that produced it.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    public GitLabAuthenticationException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri)
        : base(statusCode, message, responseBody, requestMethod, requestUri)
    {
    }
}