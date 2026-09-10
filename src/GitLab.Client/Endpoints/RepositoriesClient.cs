using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class RepositoriesClient(IGitLabApiConnection connection) : IRepositoriesClient
{
    /// <summary>GitLab answers 400 when <c>merge_base</c> is given fewer refs than this.</summary>
    private const int MinimumMergeBaseRefs = 2;

    /// <summary>GitLab caps one batch-blob request at 20 file descriptors.</summary>
    private const int MaximumBatchBlobFiles = 20;

    public IAsyncEnumerable<GitLabTreeItem> ListTreeAsync(ProjectId projectId, TreeListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tree")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabTreeItemArray,
            cancellationToken);
    }

    public Task<GitLabCompare> CompareAsync(ProjectId projectId, string fromRef, string toRef,
        long? fromProjectId = null, bool? straight = null, bool? unidiff = null,
        CancellationToken cancellationToken = default)
    {
        ValidateComparisonRefs(fromRef, toRef);

        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("compare")
                .Query("from", fromRef)
                .Query("to", toRef)
                .Query("from_project_id", fromProjectId)
                .Query("straight", straight)
                .Query("unidiff", unidiff)
                .Build(),
            GitLabJsonContext.Default.GitLabCompare,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabContributor> ListContributorsAsync(ProjectId projectId,
        ContributorListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("contributors")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabContributorArray,
            cancellationToken);
    }

    public Task<GitLabCommit> GetMergeBaseAsync(ProjectId projectId, IReadOnlyList<string> refs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refs);
        ArgumentOutOfRangeException.ThrowIfLessThan(refs.Count, MinimumMergeBaseRefs, nameof(refs));

        for (int index = 0; index < refs.Count; index++)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(refs[index], $"{nameof(refs)}[{index}]");
        }

        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("merge_base")
                .QueryRepeated("refs", refs)
                .Build(),
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiffStat> ListDiffStatsAsync(ProjectId projectId, string fromRef, string toRef,
        CancellationToken cancellationToken = default)
    {
        ValidateComparisonRefs(fromRef, toRef);

        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("diff_stats")
                .Query("from", fromRef)
                .Query("to", toRef)
                .Build(),
            GitLabJsonContext.Default.GitLabDiffStatArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabChangedPath> ListChangedPathsAsync(ProjectId projectId, string fromRef, string toRef,
        bool? findRenames = null, CancellationToken cancellationToken = default)
    {
        ValidateComparisonRefs(fromRef, toRef);

        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("changed_paths")
                .Query("from", fromRef)
                .Query("to", toRef)
                .Query("find_renames", findRenames)
                .Build(),
            GitLabJsonContext.Default.GitLabChangedPathArray,
            cancellationToken);
    }

    public Task<GitLabDivergingCommitCount> GetDivergingCommitCountsAsync(ProjectId projectId, string fromRef,
        string toRef, int? maxCount = null, CancellationToken cancellationToken = default)
    {
        ValidateComparisonRefs(fromRef, toRef);
        if (maxCount is { } count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(maxCount));
        }

        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("diverging_commits")
                .Query("from", fromRef)
                .Query("to", toRef)
                .Query("max_count", maxCount)
                .Build(),
            GitLabJsonContext.Default.GitLabDivergingCommitCount,
            cancellationToken);
    }

    public Task<GitLabChangelog> GenerateChangelogAsync(ProjectId projectId, string version, string? fromRef = null,
        string? toRef = null, DateTimeOffset? releaseDate = null, string? trailer = null, string? configFile = null,
        string? configFileRef = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("changelog")
                .Query("version", version)
                .Query("from", fromRef)
                .Query("to", toRef)
                .Query("date", releaseDate)
                .Query("trailer", trailer)
                .Query("config_file", configFile)
                .Query("config_file_ref", configFileRef)
                .Build(),
            GitLabJsonContext.Default.GitLabChangelog,
            cancellationToken);
    }

    public Task AddChangelogAsync(ProjectId projectId, AddChangelogRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.PostAsync(
            RepositoryRoute(projectId)
                .Literal("changelog")
                .Build(),
            request,
            GitLabJsonContext.Default.AddChangelogRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId,
        RepositoryArchiveOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RepositoryRoute(projectId)
                .Literal("archive")
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadSnapshotAsync(ProjectId projectId, bool? wiki = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("snapshot")
                .Query("wiki", wiki)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabBlob> GetBlobAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha);

        return connection.GetAsync(
            BlobRoute(projectId, sha).Build(),
            GitLabJsonContext.Default.GitLabBlob,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawBlobAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha);

        return connection.GetFileAsync(
            BlobRoute(projectId, sha)
                .Literal("raw")
                .Build(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<GitLabBatchBlob>> GetBlobsAsync(ProjectId projectId, BlobBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Files);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.Files.Count, MaximumBatchBlobFiles,
            nameof(request));

        for (int index = 0; index < request.Files.Count; index++)
        {
            BlobBatchFile file = request.Files[index];
            ArgumentNullException.ThrowIfNull(file);
            ArgumentException.ThrowIfNullOrWhiteSpace(file.Path,
                $"{nameof(request)}.{nameof(request.Files)}[{index}].{nameof(file.Path)}");
        }

        // One unpaginated array, so there is nothing for GetPagedAsync to follow - the result is
        // materialized rather than streamed.
        return await connection.PostAsync(
                RepositoryRoute(projectId)
                    .Literal("blobs")
                    .Literal("batch")
                    .Build(),
                request,
                GitLabJsonContext.Default.BlobBatchRequest,
                GitLabJsonContext.Default.GitLabBatchBlobArray,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<GitLabRepositoryHealth> GetHealthAsync(ProjectId projectId, bool? generate = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RepositoryRoute(projectId)
                .Literal("health")
                .Query("generate", generate)
                .Build(),
            GitLabJsonContext.Default.GitLabRepositoryHealth,
            cancellationToken);
    }

    public Task<GitLabCommit> UpdateSubmoduleAsync(ProjectId projectId, string submodule,
        UpdateSubmoduleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(submodule);
        ArgumentNullException.ThrowIfNull(request);

        return connection.PutAsync(
            RepositoryRoute(projectId)
                .Literal("submodules")
                .Escaped(submodule)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateSubmoduleRequest,
            GitLabJsonContext.Default.GitLabCommit,
            cancellationToken);
    }

    private static GitLabRouteBuilder RepositoryRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("repository");
    }

    /// <summary>
    ///     The SHA is escaped rather than appended verbatim for the same reason every other caller-supplied
    ///     ref is: nothing stops a caller passing a name here, and an unescaped slash silently changes the
    ///     route.
    /// </summary>
    private static GitLabRouteBuilder BlobRoute(ProjectId projectId, string sha)
    {
        return RepositoryRoute(projectId)
            .Literal("blobs")
            .Escaped(sha);
    }

    private static void ValidateComparisonRefs(string fromRef, string toRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromRef);
        ArgumentException.ThrowIfNullOrWhiteSpace(toRef);
    }
}