using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Namespaces" API area (<c>/namespaces</c>) - the umbrella GitLab exposes over
///     both groups and personal (user) namespaces, plus the internal, license-aware project listing
///     under <c>/internal/gitlab_subscriptions/namespaces/:id/projects</c>.
/// </summary>
public interface INamespacesClient
{
    /// <summary>
    ///     Streams every namespace visible to the caller (<c>GET /namespaces</c>) - every namespace on the
    ///     instance for an administrator, or just the caller's own otherwise.
    /// </summary>
    IAsyncEnumerable<GitLabNamespace> ListAsync(NamespaceListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one namespace by numeric ID or URL-encoded full path (<c>GET /namespaces/:id</c>).</summary>
    Task<GitLabNamespace> GetAsync(GroupId namespaceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether <paramref name="path" /> is still available for a new namespace
    ///     (<c>GET /namespaces/:id/exists</c>) - unlike every other method here, <paramref name="path" />
    ///     is a candidate path being tested, not necessarily an existing namespace.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <param name="parentId">
    ///     Restricts the check to children of this namespace. Leaving it unset checks top-level namespaces.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabNamespaceExistence> ExistsAsync(string path, long? parentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves the GitLab subscription for a namespace (<c>GET /namespaces/:id/gitlab_subscription</c>).</summary>
    Task<GitLabNamespaceSubscription> GetSubscriptionAsync(GroupId namespaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every namespace GitLab has excluded from storage-limit enforcement
    ///     (<c>GET /namespaces/storage/limit_exclusions</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNamespaceStorageLimitExclusion> ListStorageLimitExclusionsAsync(
        NamespaceStorageLimitExclusionListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Excludes a namespace from storage-limit enforcement
    ///     (<c>POST /namespaces/:id/storage/limit_exclusion</c>).
    /// </summary>
    Task<GitLabNamespaceStorageLimitExclusion> CreateStorageLimitExclusionAsync(GroupId namespaceId,
        CreateNamespaceStorageLimitExclusionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reverses <see cref="CreateStorageLimitExclusionAsync" /> (
    ///     <c>DELETE /namespaces/:id/storage/limit_exclusion</c>).
    /// </summary>
    Task DeleteStorageLimitExclusionAsync(GroupId namespaceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every project inside a namespace, including its subgroups' projects, with license data
    ///     folded into each entry (<c>GET /internal/gitlab_subscriptions/namespaces/:id/projects</c>). An
    ///     internal endpoint CustomersDot uses to check open-source program compliance without one API call
    ///     per project.
    /// </summary>
    IAsyncEnumerable<GitLabNamespaceProject> ListProjectsAsync(GroupId namespaceId,
        NamespaceProjectListOptions? options = null, CancellationToken cancellationToken = default);
}