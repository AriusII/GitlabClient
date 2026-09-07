using System.Text.Json;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps a handful of GitLab REST operations that carry no OpenAPI tag and share no natural home
///     with any other resource client: the Gitaly-internal object pool RPC, and the two Swagger
///     compatible API description endpoints.
/// </summary>
public interface IInternalClient
{
    /// <summary>
    ///     Lists the members of a Gitaly object pool (<c>GET /internal/gitaly/object_pool_members</c>). A
    ///     Gitaly-internal RPC, not a route an ordinary API consumer calls. The spec declares no response
    ///     schema, so the answer is a raw <see cref="JsonElement" /> rather than an invented shape.
    /// </summary>
    Task<JsonElement> ListGitalyObjectPoolMembersAsync(IReadOnlyList<string> diskPaths, string storage,
        bool? upstreamOnly = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the Swagger compatible description of GitLab's whole API (<c>GET /swagger_doc</c>). The
    ///     spec declares no response schema, so the answer is a raw <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> GetSwaggerDocumentationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the Swagger compatible description for one mounted API area
    ///     (<c>GET /swagger_doc/:name</c>). The spec declares no response schema, so the answer is a raw
    ///     <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> GetSwaggerDocumentationAsync(string name, string? locale = null,
        CancellationToken cancellationToken = default);
}