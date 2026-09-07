using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

internal sealed class DependencyProxyRepository(IGitLabApiConnection connection) : IDependencyProxyRepository
{
    public Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("dependency_proxy").Literal("cache")
                .Build(),
            cancellationToken);
    }
}