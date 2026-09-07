using System.Text.Json;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Container registry, sitting between the public
///     <c>IContainerRegistryClient</c> controller and <c>IContainerRegistryRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IContainerRegistryService
{
    Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default);
}