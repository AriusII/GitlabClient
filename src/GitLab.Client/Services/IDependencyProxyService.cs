using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Dependency proxy, sitting between the public
///     <c>IDependencyProxyClient</c> controller and <c>IDependencyProxyRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IDependencyProxyService
{
    Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default);
}