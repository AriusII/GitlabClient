using System.Text.Json;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Internal, sitting between the public <c>IInternalClient</c>
///     controller and <c>IInternalRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IInternalService
{
    Task<JsonElement> ListGitalyObjectPoolMembersAsync(IReadOnlyList<string> diskPaths, string storage,
        bool? upstreamOnly = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetSwaggerDocumentationAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetSwaggerDocumentationAsync(string name, string? locale = null,
        CancellationToken cancellationToken = default);
}