using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Commit statuses" API area - the external-CI integration surface. The two routes are
///     deliberately asymmetric: reads come from <c>/projects/:id/repository/commits/:sha/statuses</c> and
///     writes go to <c>/projects/:id/statuses/:sha</c>.
/// </summary>
public interface ICommitStatusesClient
{
    /// <summary>
    ///     Streams the statuses reported against one commit. By default GitLab returns only the latest status
    ///     per name; set <see cref="CommitStatusListOptions.All" /> to see retries too.
    /// </summary>
    IAsyncEnumerable<GitLabCommitStatus> ListAsync(ProjectId projectId, string sha,
        CommitStatusListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or updates the status of a commit, as a job in an "external" stage. GitLab keys the record on
    ///     (sha, name/context, pipeline_id), so posting twice with the same label updates rather than duplicates.
    ///     For a commit that belongs to a merge request, target the commit on the MR's source branch - a status
    ///     posted against the merge-result commit does not show up.
    /// </summary>
    Task<GitLabCommitStatus> CreateAsync(ProjectId projectId, string sha, CreateCommitStatusRequest request,
        CancellationToken cancellationToken = default);
}