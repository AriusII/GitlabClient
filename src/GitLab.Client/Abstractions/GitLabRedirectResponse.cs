using System.Net;
using System.Net.Http.Headers;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The outcome of a request GitLab answers with a redirect rather than a body - the PyPI package-proxy
///     forwarding route (<c>GET /projects/{id}/packages/pypi/forward/{package_name}/{upstream_path}</c>) is
///     the first of these: it protects the actual package location behind a <c>302 Found</c> and a
///     <c>Location</c> header instead of streaming the file itself.
/// </summary>
/// <remarks>
///     The connection's primary handler runs with <c>AllowAutoRedirect = false</c> precisely so a
///     redirect like this reaches the caller instead of being followed transparently underneath it (see
///     the class remarks on <c>GitLabAuthenticationHandler</c>) - a transparently-followed cross-host
///     redirect could silently leak the caller's <c>PRIVATE-TOKEN</c> to whatever host <c>Location</c>
///     names. A 3xx status is therefore treated as this method's successful answer, not as a failure:
///     every other non-success status still throws the typed exception it would on any other verb.
/// </remarks>
public sealed class GitLabRedirectResponse
{
    /// <summary>Creates a redirect result. Public so consumers can build an <see cref="IGitLabApiConnection" /> test double.</summary>
    /// <param name="statusCode">The status GitLab answered with.</param>
    /// <param name="location">The parsed <c>Location</c> header, or null when GitLab did not send one.</param>
    /// <param name="headers">The response headers, keyed case-insensitively.</param>
    public GitLabRedirectResponse(HttpStatusCode statusCode, Uri? location,
        IReadOnlyDictionary<string, IReadOnlyList<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        StatusCode = statusCode;
        Location = location;
        Headers = headers;
    }

    /// <summary>The status GitLab answered with - typically <c>302 Found</c>, but any 3xx is possible.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    ///     Where GitLab is redirecting to, already resolved against the request if it was sent as a
    ///     relative reference. Null on the (unexpected, but not thrown-on) case where a non-3xx status came
    ///     back with no <c>Location</c> header.
    /// </summary>
    public Uri? Location { get; }

    /// <summary>Every response header, entity headers included, keyed case-insensitively.</summary>
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
    internal static GitLabRedirectResponse FromResponse(HttpResponseMessage response)
    {
        Dictionary<string, IReadOnlyList<string>> headers = new(StringComparer.OrdinalIgnoreCase);

        Collect(response.Headers, headers);
        Collect(response.Content.Headers, headers);

        return new GitLabRedirectResponse(response.StatusCode, response.Headers.Location, headers);
    }

    private static void Collect(HttpHeaders source, Dictionary<string, IReadOnlyList<string>> destination)
    {
        foreach (KeyValuePair<string, IEnumerable<string>> header in source)
        {
            destination[header.Key] = [.. header.Value];
        }
    }
}