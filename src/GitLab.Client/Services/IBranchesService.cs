using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Branches, sitting between the public <c>IBranchesClient</c>
///     controller and <c>IBranchesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IBranchesService
{
    Task<GitLabBranch> GetAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBranch> ListAsync(ProjectId projectId, BranchListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBranch> CreateAsync(ProjectId projectId, CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    Task DeleteMergedAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    Task<GitLabBranch> ProtectAsync(ProjectId projectId, string branchName, ProtectSingleBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBranch> UnprotectAsync(ProjectId projectId, string branchName,
        CancellationToken cancellationToken = default);
}