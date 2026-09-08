using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Namespaces resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Every <c>{id}</c> here is the ID-or-URL-encoded-path shape GitLab documents for a namespace,
///         which is byte-for-byte the same convention <see cref="GroupId" /> already implements for
///         groups - a namespace's numeric ID is its underlying group's (or user's) ID, and the string
///         form is the same URL-encoded full path. Introducing a parallel <c>NamespaceId</c> type would
///         duplicate <see cref="GroupId" /> with no behavioural difference, so this resource reuses it
///         rather than adding one. The one exception is <see cref="ExistsAsync" />: GitLab's spec types
///         that <c>{id}</c> as a bare string only, because it is a candidate path being checked for
///         availability - not necessarily an existing namespace - so it takes a plain <see cref="string" />
///         instead.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(INamespacesService), typeof(INamespacesClient))]
internal interface INamespacesRepository
{
    IAsyncEnumerable<GitLabNamespace> ListAsync(NamespaceListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabNamespace> GetAsync(GroupId namespaceId, CancellationToken cancellationToken = default);

    Task<GitLabNamespace> UpdateAsync(GroupId namespaceId, UpdateNamespaceRequest request,
        CancellationToken cancellationToken = default);

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