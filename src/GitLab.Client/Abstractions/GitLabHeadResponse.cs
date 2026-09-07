using System.Net;
using System.Net.Http.Headers;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The outcome of a <c>HEAD</c> probe. GitLab offers a handful of these as cheap existence checks -
///     <c>HEAD /projects/{id}/repository/branches/{branch}</c>,
///     <c>HEAD /projects/{id}/repository/files/{path}</c> - answering 404 for "no" rather than as an error,
///     and returning the resource's metadata in <c>X-Gitlab-*</c> headers instead of a body.
/// </summary>
/// <remarks>
///     A 404 is therefore reported as <see cref="Exists" /> being false, not as a
///     <see cref="Exceptions.GitLabNotFoundException" />. Every other failure still throws the typed
///     exception it would on any other verb, so a 401 or a 403 can never be mistaken for "does not exist".
/// </remarks>
public sealed class GitLabHeadResponse
{
    /// <summary>Creates a probe result. Public so consumers can build an <see cref="IGitLabApiConnection" /> test double.</summary>
    /// <param name="statusCode">The status GitLab answered the probe with.</param>
    /// <param name="headers">The response headers, keyed case-insensitively.</param>
    public GitLabHeadResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IReadOnlyList<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        StatusCode = statusCode;
        Headers = headers;
    }

    /// <summary>True unless GitLab answered <c>404 Not Found</c>.</summary>
    public bool Exists => StatusCode != HttpStatusCode.NotFound;

    /// <summary>The status GitLab answered the probe with.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    ///     Every response header, entity headers included, keyed case-insensitively. This is where the answer
    ///     actually lives for a <c>HEAD</c> on a repository file: <c>X-Gitlab-Blob-Id</c>,
    ///     <c>X-Gitlab-Size</c>, <c>X-Gitlab-Last-Commit-Id</c> and friends.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }

    /// <summary>Reads the first value of a header, or null when the response did not carry it.</summary>
    /// <param name="name">The header name, matched case-insensitively.</param>
    /// <returns>The first value sent for <paramref name="name" />, or null.</returns>
    public string? GetHeaderValue(string name)
    {
        return Headers.TryGetValue(name, out IReadOnlyList<string>? values) && values.Count > 0
            ? values[0]
            : null;
    }

    /// <summary>Collapses a response's header and entity-header collections into one case-insensitive map.</summary>
    internal static GitLabHeadResponse FromResponse(HttpResponseMessage response)
    {
        Dictionary<string, IReadOnlyList<string>> headers =
            new(StringComparer.OrdinalIgnoreCase);

        Collect(response.Headers, headers);
        Collect(response.Content.Headers, headers);

        return new GitLabHeadResponse(response.StatusCode, headers);
    }

    private static void Collect(
        HttpHeaders source,
        Dictionary<string, IReadOnlyList<string>> destination)
    {
        foreach (KeyValuePair<string, IEnumerable<string>> header in source)
        {
            destination[header.Key] = [.. header.Value];
        }
    }
}