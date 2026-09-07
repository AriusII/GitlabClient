using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Branches resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IBranchesService), typeof(IBranchesClient))]
internal interface IBranchesRepository
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