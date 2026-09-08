using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Commits, sitting between the public <c>ICommitsClient</c>
///     controller and <c>ICommitsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ICommitsService
{
    Task<GitLabCommit> GetAsync(ProjectId projectId, string sha, bool? stats = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommit> ListAsync(ProjectId projectId, CommitListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCommit> CreateAsync(ProjectId projectId, CreateCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabCommit> CherryPickAsync(ProjectId projectId, string sha, CherryPickCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabCommit> RevertAsync(ProjectId projectId, string sha, RevertCommitRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommitComment> ListCommentsAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    Task<GitLabCommitComment> CreateCommentAsync(ProjectId projectId, string sha,
        CreateCommitCommentRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, string sha, CommitDiffOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, string sha,
        CommitMergeRequestListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommitRef> ListRefsAsync(ProjectId projectId, string sha,
        CommitRefListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabCommitSequence> GetSequenceAsync(ProjectId projectId, string sha, bool? firstParent = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCommitSignature> GetSignatureAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    Task<GitLabWebCommitsPublicKey> GetWebCommitsPublicKeyAsync(CancellationToken cancellationToken = default);
}