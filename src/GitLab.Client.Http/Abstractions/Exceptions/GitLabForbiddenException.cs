using System.Net;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     The credentials were accepted but are not allowed to perform the request (HTTP 403): insufficient role on
///     the project or group, a token scope that does not cover the endpoint, or a policy that blocks it. Retrying
///     will not help; the token needs more rights, or the caller needs a different one.
/// </summary>
public sealed class GitLabForbiddenException : GitLabApiException
{
    /// <summary>Initializes a new instance with a default message and no GitLab response context.</summary>
    public GitLabForbiddenException()
    {
    }

    /// <summary>Initializes a new instance with the supplied message and no GitLab response context.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabForbiddenException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance from a GitLab response, carrying the request that produced it.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    public GitLabForbiddenException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri)
        : base(statusCode, message, responseBody, requestMethod, requestUri)
    {
    }
}