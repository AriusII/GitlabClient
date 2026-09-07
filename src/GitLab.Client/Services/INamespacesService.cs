using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Namespaces, sitting between the public
///     <c>INamespacesClient</c> controller and <c>INamespacesRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once this resource
///     needs more than pass-through.
/// </summary>
internal interface INamespacesService
{
    IAsyncEnumerable<GitLabNamespace> ListAsync(NamespaceListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabNamespace> GetAsync(GroupId namespaceId, CancellationToken cancellationToken = default);

    Task<GitLabNamespaceExistence> ExistsAsync(string path, long? parentId = null,
        CancellationToken cancellationToken = default);

    Task<GitLabNamespaceSubscription> GetSubscriptionAsync(GroupId namespaceId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNamespaceStorageLimitExclusion> ListStorageLimitExclusionsAsync(
        NamespaceStorageLimitExclusionListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNamespaceStorageLimitExclusion> CreateStorageLimitExclusionAsync(GroupId namespaceId,
        CreateNamespaceStorageLimitExclusionRequest request, CancellationToken cancellationToken = default);

    Task DeleteStorageLimitExclusionAsync(GroupId namespaceId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNamespaceProject> ListProjectsAsync(GroupId namespaceId,
        NamespaceProjectListOptions? options = null, CancellationToken cancellationToken = default);
}