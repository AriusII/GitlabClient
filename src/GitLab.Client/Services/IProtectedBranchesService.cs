using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ProtectedBranches, sitting between the public
///     <c>IProtectedBranchesClient</c> controller and <c>IProtectedBranchesRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IProtectedBranchesService
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