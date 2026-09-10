using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class CommitsClient(IGitLabApiConnection connection) : ICommitsClient
{
    // POST /projects/:id/repository/commits declares one required binary part, named "file", in the
    // GitLab 19.4 OpenAPI schema. Pin the part name so a caller cannot accidentally send an ignored field.
    private const string FileFieldName = "file";

    public Task<GitLabCommit> GetAsync(ProjectId projectId, string sha, bool? stats = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            CommitRoute(projectId, sha)
                .Query("stats", stats)
                .Build(),
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommit> ListAsync(ProjectId projectId, CommitListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitsRoute(projectId)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabCommitArray,
            cancellationToken);
    }

    public Task<GitLabCommit> CreateAsync(ProjectId projectId, CreateCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitsRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateCommitRequest,
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public Task<GitLabCommit> CreateAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            CommitsRoute(projectId).Build(),
            file with { FieldName = FileFieldName },
            null,
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public Task<GitLabCommit> CherryPickAsync(ProjectId projectId, string sha, CherryPickCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitRoute(projectId, sha)
                .Literal("cherry_pick")
                .Build(),
            request,
            GitLabJsonContext.Default.CherryPickCommitRequest,
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public async Task<GitLabCommitOperationResult> CherryPickWithResultAsync(ProjectId projectId, string sha,
        CherryPickCommitRequest request, CancellationToken cancellationToken = default)
    {
        JsonElement response = await connection.PostAsync(
                CommitRoute(projectId, sha)
                    .Literal("cherry_pick")
                    .Build(),
                request,
                GitLabJsonContext.Default.CherryPickCommitRequest,
                GitLabJsonContext.Default.JsonElement,
                cancellationToken)
            .ConfigureAwait(false);

        return ToOperationResult(response);
    }

    public Task<GitLabCommit> RevertAsync(ProjectId projectId, string sha, RevertCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitRoute(projectId, sha)
                .Literal("revert")
                .Build(),
            request,
            GitLabJsonContext.Default.RevertCommitRequest,
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public async Task<GitLabCommitOperationResult> RevertWithResultAsync(ProjectId projectId, string sha,
        RevertCommitRequest request, CancellationToken cancellationToken = default)
    {
        JsonElement response = await connection.PostAsync(
                CommitRoute(projectId, sha)
                    .Literal("revert")
                    .Build(),
                request,
                GitLabJsonContext.Default.RevertCommitRequest,
                GitLabJsonContext.Default.JsonElement,
                cancellationToken)
            .ConfigureAwait(false);

        return ToOperationResult(response);
    }

    public IAsyncEnumerable<GitLabCommitComment> ListCommentsAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitRoute(projectId, sha)
                .Literal("comments")
                .Build(),
            GitLabJsonContext.Default.GitLabCommitCommentArray,
            cancellationToken);
    }

    public Task<GitLabCommitComment> CreateCommentAsync(ProjectId projectId, string sha,
        CreateCommitCommentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitRoute(projectId, sha)
                .Literal("comments")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateCommitCommentRequest,
            GitLabJsonContext.Default.GitLabCommitComment,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, string sha,
        CommitDiffOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitRoute(projectId, sha)
                .Literal("diff")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDiffArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, string sha,
        CommitMergeRequestListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitRoute(projectId, sha)
                .Literal("merge_requests")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommitRef> ListRefsAsync(ProjectId projectId, string sha,
        CommitRefListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitRoute(projectId, sha)
                .Literal("refs")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabCommitRefArray,
            cancellationToken);
    }

    public Task<GitLabCommitSequence> GetSequenceAsync(ProjectId projectId, string sha, bool? firstParent = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            CommitRoute(projectId, sha)
                .Literal("sequence")
                .Query("first_parent", firstParent)
                .Build(),
            GitLabJsonContext.Default.GitLabCommitSequence,
            cancellationToken);
    }

    public Task<GitLabCommitSignature> GetSignatureAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            CommitRoute(projectId, sha)
                .Literal("signature")
                .Build(),
            GitLabJsonContext.Default.GitLabCommitSignature,
            cancellationToken);
    }

    public Task<GitLabWebCommitsPublicKey> GetWebCommitsPublicKeyAsync(
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("web_commits")
                .Literal("public_key")
                .Build(),
            GitLabJsonContext.Default.GitLabWebCommitsPublicKey,
            cancellationToken);
    }

    private static GitLabRouteBuilder CommitsRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("repository")
            .Literal("commits");
    }

    /// <summary>
    ///     The SHA is <see cref="GitLabRouteBuilder.Escaped(string)" /> rather than appended verbatim
    ///     because every one of these routes also accepts a branch or tag name in its place, and those
    ///     routinely contain a slash (<c>release/1.0</c>).
    /// </summary>
    private static GitLabRouteBuilder CommitRoute(ProjectId projectId, string sha)
    {
        return CommitsRoute(projectId).Escaped(sha);
    }

    private static GitLabCommitOperationResult ToOperationResult(JsonElement response)
    {
        if (response.TryGetProperty("dry_run", out JsonElement dryRun))
        {
            return new GitLabCommitOperationResult
            {
                DryRun = dryRun.GetString()
                         ?? throw new JsonException("GitLab returned a null dry_run result for a commit operation.")
            };
        }

        return new GitLabCommitOperationResult
        {
            Commit = response.Deserialize(GitLabJsonContext.Default.GitLabCommit)
                     ?? throw new JsonException("GitLab returned an empty commit operation result.")
        };
    }
}