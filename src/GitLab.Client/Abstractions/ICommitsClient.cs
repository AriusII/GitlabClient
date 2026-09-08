using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Commits" API area (<c>/projects/:id/repository/commits</c>).</summary>
public interface ICommitsClient
{
    /// <summary>Retrieves a single commit (<c>GET /projects/:id/repository/commits/:sha</c>).</summary>
    /// <param name="projectId">The project the commit belongs to.</param>
    /// <param name="sha">A commit SHA, or the name of a branch or tag.</param>
    /// <param name="stats">
    ///     Includes <see cref="GitLabCommit.Stats" /> in the response. GitLab includes it by default;
    ///     pass <see langword="false" /> to omit it from the response.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabCommit> GetAsync(ProjectId projectId, string sha, bool? stats = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a project's commits (<c>GET /projects/:id/repository/commits</c>), optionally scoped to
    ///     a branch or tag and a date range via <see cref="CommitListOptions" />.
    /// </summary>
    IAsyncEnumerable<GitLabCommit> ListAsync(ProjectId projectId, CommitListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates one commit that applies several file operations at once
    ///     (<c>POST /projects/:id/repository/commits</c>). Every action in
    ///     <see cref="CreateCommitRequest.Actions" /> lands in a single commit, so the set is applied
    ///     atomically.
    /// </summary>
    Task<GitLabCommit> CreateAsync(ProjectId projectId, CreateCommitRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cherry-picks a commit onto another branch
    ///     (<c>POST /projects/:id/repository/commits/:sha/cherry_pick</c>). <paramref name="sha" /> may
    ///     also be a branch or tag name and is URL-encoded for you.
    /// </summary>
    Task<GitLabCommit> CherryPickAsync(ProjectId projectId, string sha, CherryPickCommitRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reverts a commit on a branch (<c>POST /projects/:id/repository/commits/:sha/revert</c>).
    /// </summary>
    Task<GitLabCommit> RevertAsync(ProjectId projectId, string sha, RevertCommitRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a commit's comments (<c>GET /projects/:id/repository/commits/:sha/comments</c>).
    /// </summary>
    IAsyncEnumerable<GitLabCommitComment> ListCommentsAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Comments on a commit (<c>POST /projects/:id/repository/commits/:sha/comments</c>). Supply the
    ///     request's path, line and line type together to anchor the comment to a line of the diff.
    /// </summary>
    Task<GitLabCommitComment> CreateCommentAsync(ProjectId projectId, string sha,
        CreateCommitCommentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a commit's diff, one entry per changed file
    ///     (<c>GET /projects/:id/repository/commits/:sha/diff</c>).
    /// </summary>
    IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, string sha, CommitDiffOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge requests a commit belongs to
    ///     (<c>GET /projects/:id/repository/commits/:sha/merge_requests</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, string sha,
        CommitMergeRequestListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the branches and tags a commit has been pushed to
    ///     (<c>GET /projects/:id/repository/commits/:sha/refs</c>).
    /// </summary>
    IAsyncEnumerable<GitLabCommitRef> ListRefsAsync(ProjectId projectId, string sha,
        CommitRefListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the commits that precede this one on its own history
    ///     (<c>GET /projects/:id/repository/commits/:sha/sequence</c>) - the API equivalent of
    ///     <c>git rev-list --count</c>.
    /// </summary>
    /// <param name="projectId">The project the commit belongs to.</param>
    /// <param name="sha">The commit SHA.</param>
    /// <param name="firstParent">Follows only the first parent of each merge commit.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The commit count.</returns>
    Task<GitLabCommitSequence> GetSequenceAsync(ProjectId projectId, string sha, bool? firstParent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a commit's signature (<c>GET /projects/:id/repository/commits/:sha/signature</c>). GitLab
    ///     answers <c>404</c> for an unsigned commit, which surfaces as a
    ///     <see cref="Exceptions.GitLabNotFoundException" />.
    /// </summary>
    Task<GitLabCommitSignature> GetSignatureAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the instance-wide key GitLab signs web-UI and API-created commits with
    ///     (<c>GET /web_commits/public_key</c>). Instance-scoped rather than project-scoped, and
    ///     introduced in GitLab 17.4.
    /// </summary>
    Task<GitLabWebCommitsPublicKey> GetWebCommitsPublicKeyAsync(CancellationToken cancellationToken = default);
}