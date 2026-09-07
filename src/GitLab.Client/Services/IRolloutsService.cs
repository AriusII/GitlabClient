using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Rollouts, sitting between the public <c>IRolloutsClient</c>
///     controller and <c>IRolloutsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IRolloutsService
{
    Task<GitLabCdRollout> IngestEventAsync(long id, IngestRolloutEventRequest request,
        CancellationToken cancellationToken = default);
}