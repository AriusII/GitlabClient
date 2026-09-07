using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Merge trains, sitting between the public
///     <c>IMergeTrainsClient</c> controller and <c>IMergeTrainsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IMergeTrainsService
{
    IAsyncEnumerable<GitLabMergeTrainCar> ListAsync(ProjectId projectId, MergeTrainListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeTrainCar> ListForTargetBranchAsync(ProjectId projectId, string targetBranch,
        MergeTrainListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabMergeTrainCar> GetStatusAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeTrainCar> AddMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        AddToMergeTrainRequest request, CancellationToken cancellationToken = default);
}