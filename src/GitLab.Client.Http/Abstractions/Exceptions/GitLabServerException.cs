using System.Net;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     GitLab - or something in front of it - failed to handle a request that was itself well formed (any
///     HTTP 5xx). Retrying with backoff is usually appropriate; <see cref="GitLabApiException.IsTransient" /> is
///     true. One type covers the whole range on purpose: 500, 502, 503 and 504 differ in cause but not in what a
///     caller can do about them.
/// </summary>
public sealed class GitLabServerException : GitLabApiException
{
    /// <summary>Initializes a new instance with a default message and no GitLab response context.</summary>
    public GitLabServerException()
    {
    }

    /// <summary>Initializes a new instance with the supplied message and no GitLab response context.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabServerException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabServerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance from a GitLab response, carrying the request that produced it.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    public GitLabServerException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri)
        : base(statusCode, message, responseBody, requestMethod, requestUri)
    {
    }
}