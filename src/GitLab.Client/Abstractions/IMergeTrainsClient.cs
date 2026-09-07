using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Merge trains" API area (<c>/projects/:id/merge_trains</c>) - the queue that
///     merges approved merge requests one at a time, each tested against the result of the ones ahead
///     of it.
///     <para>
///         Every endpoint here returns <see cref="GitLabMergeTrainCar" /> objects, one per queued merge
///         request rather than one per train: a project runs one train per target branch, so
///         <see cref="ListAsync" /> mixes cars from several trains and
///         <see cref="ListForTargetBranchAsync" /> narrows to one.
///     </para>
///     <para>
///         Merge trains are a GitLab Premium feature and must be enabled on the project. Without them
///         GitLab answers <c>403</c> or <c>404</c> rather than an empty list.
///     </para>
/// </summary>
public interface IMergeTrainsClient
{
    /// <summary>Streams every car on every one of the project's merge trains.</summary>
    IAsyncEnumerable<GitLabMergeTrainCar> ListAsync(ProjectId projectId, MergeTrainListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the cars of the one train targeting <paramref name="targetBranch" />. Branch names legally
    ///     contain <c>/</c> (<c>release/2.0</c>); the route builder percent-encodes the name, so pass it raw.
    /// </summary>
    IAsyncEnumerable<GitLabMergeTrainCar> ListForTargetBranchAsync(ProjectId projectId, string targetBranch,
        MergeTrainListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the merge train status of one merge request, addressed by its project-scoped
    ///     <paramref name="mergeRequestIid" /> rather than its global id.
    /// </summary>
    Task<GitLabMergeTrainCar> GetStatusAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a merge request to the train for its target branch, returning the car GitLab created for it.
    ///     Set <see cref="AddToMergeTrainRequest.Sha" /> to refuse the queueing if someone has pushed to the
    ///     source branch since it was read.
    /// </summary>
    Task<GitLabMergeTrainCar> AddMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        AddToMergeTrainRequest request, CancellationToken cancellationToken = default);
}