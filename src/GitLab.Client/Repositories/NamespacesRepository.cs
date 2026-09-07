using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class NamespacesRepository(IGitLabApiConnection connection) : INamespacesRepository
{
    public IAsyncEnumerable<GitLabNamespace> ListAsync(NamespaceListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("namespaces").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabNamespaceArray,
            cancellationToken);
    }

    public Task<GitLabNamespace> GetAsync(GroupId namespaceId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("namespaces").Segment(namespaceId).Build(),
            GitLabJsonContext.Default.GitLabNamespace,
            cancellationToken);
    }

    public Task<GitLabNamespaceExistence> ExistsAsync(string path, long? parentId = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("namespaces").Escaped(path).Literal("exists").Query("parent_id", parentId)
                .Build(),
            GitLabJsonContext.Default.GitLabNamespaceExistence,
            cancellationToken);
    }

    public Task<GitLabNamespaceSubscription> GetSubscriptionAsync(GroupId namespaceId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("namespaces").Segment(namespaceId).Literal("gitlab_subscription").Build(),
            GitLabJsonContext.Default.GitLabNamespaceSubscription,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNamespaceStorageLimitExclusion> ListStorageLimitExclusionsAsync(
        NamespaceStorageLimitExclusionListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("namespaces").Literal("storage").Literal("limit_exclusions")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabNamespaceStorageLimitExclusionArray,
            cancellationToken);
    }

    public Task<GitLabNamespaceStorageLimitExclusion> CreateStorageLimitExclusionAsync(GroupId namespaceId,
        CreateNamespaceStorageLimitExclusionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("namespaces").Segment(namespaceId).Literal("storage")
                .Literal("limit_exclusion").Build(),
            request,
            GitLabJsonContext.Default.CreateNamespaceStorageLimitExclusionRequest,
            GitLabJsonContext.Default.GitLabNamespaceStorageLimitExclusion,
            cancellationToken);
    }

    public Task DeleteStorageLimitExclusionAsync(GroupId namespaceId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("namespaces").Segment(namespaceId).Literal("storage")
                .Literal("limit_exclusion").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNamespaceProject> ListProjectsAsync(GroupId namespaceId,
        NamespaceProjectListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("internal").Literal("gitlab_subscriptions").Literal("namespaces")
                .Segment(namespaceId).Literal("projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabNamespaceProjectArray,
            cancellationToken);
    }
}