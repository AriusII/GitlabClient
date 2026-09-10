using System.Collections.ObjectModel;
using System.Net;

namespace GitLab.Client.Abstractions.Exceptions;

/// <summary>
///     GitLab refused the request payload (HTTP 400 or 422). Both codes mean the same thing to a caller - fix the
///     input - so both map here; <see cref="GitLabApiException.StatusCode" /> still tells them apart.
/// </summary>
/// <remarks>
///     <see cref="Errors" /> is populated when GitLab returned its structured form,
///     <c>{"message":{"title":["can't be blank"]}}</c>. It is empty - never null - when GitLab returned a
///     plain-string message instead, which is the common shape for 400.
/// </remarks>
public sealed class GitLabValidationException : GitLabApiException
{
    /// <summary>Initializes a new instance with a default message, no GitLab response context and no field errors.</summary>
    public GitLabValidationException()
    {
        Errors = ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <summary>Initializes a new instance with the supplied message and no field errors.</summary>
    /// <param name="message">The message that describes the error.</param>
    public GitLabValidationException(string message)
        : base(message)
    {
        Errors = ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <summary>Initializes a new instance with the supplied message and inner exception, and no field errors.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current one.</param>
    public GitLabValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <summary>Initializes a new instance from a GitLab response, carrying the request and the field errors.</summary>
    /// <param name="statusCode">The HTTP status code GitLab responded with.</param>
    /// <param name="message">The message GitLab reported, or a synthesized one when it reported none.</param>
    /// <param name="responseBody">The raw response body, when GitLab sent one.</param>
    /// <param name="requestMethod">The verb of the request that failed.</param>
    /// <param name="requestUri">The URI of the request that failed.</param>
    /// <param name="errors">GitLab's per-field validation errors; null is treated as none.</param>
    public GitLabValidationException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        HttpMethod? requestMethod,
        Uri? requestUri,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? errors)
        : base(statusCode, message, responseBody, requestMethod, requestUri)
    {
        Errors = errors ?? ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <summary>
    ///     GitLab's field name mapped to the messages it reported for that field, e.g. <c>title</c> to
    ///     <c>["can't be blank"]</c>. Keys are GitLab wire names and are compared ordinally.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; }
}