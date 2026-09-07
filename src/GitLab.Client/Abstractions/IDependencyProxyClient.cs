using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Dependency proxy" API area
///     (<c>/groups/:id/dependency_proxy/cache</c>) - the cache of upstream images and packages a group
///     proxies.
/// </summary>
public interface IDependencyProxyClient
{
    /// <summary>
    ///     Schedules every cached manifest and blob in a group's dependency proxy for deletion. GitLab
    ///     answers <c>202 Accepted</c> and does the work in the background, so the returned task completing
    ///     means the purge was queued, not that the cache is already empty. Requires the Owner role on the
    ///     group.
    /// </summary>
    Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default);
}