using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ProtectedBranches resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Project and group protected branches are separate GitLab route families with near-identical
///         shapes. The group half carries a <c>...ForGroupAsync</c> suffix rather than overloading the
///         project methods: <see cref="ProjectId" /> and <see cref="GroupId" /> both convert implicitly
///         from a number and from a path, so same-named overloads would make every literal call site
///         ambiguous.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IProtectedBranchesService), typeof(IProtectedBranchesClient))]
internal interface IProtectedBranchesRepository
{
    IAsyncEnumerable<GitLabProtectedBranch> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> ProtectAsync(ProjectId projectId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> UpdateAsync(ProjectId projectId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default);

    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProtectedBranch> ListForGroupAsync(GroupId groupId,
        GroupProtectedBranchListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> GetForGroupAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> ProtectForGroupAsync(GroupId groupId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedBranch> UpdateForGroupAsync(GroupId groupId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default);

    Task UnprotectForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default);
}