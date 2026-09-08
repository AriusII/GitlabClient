using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Commits resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICommitsService), typeof(ICommitsClient))]
internal interface ICommitsRepository
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