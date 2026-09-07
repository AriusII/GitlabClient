using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Repositories, sitting between the public
///     <c>IRepositoriesClient</c> controller and <c>IRepositoriesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IRepositoriesService
{
    IAsyncEnumerable<GitLabTreeItem> ListTreeAsync(ProjectId projectId, TreeListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCompare> CompareAsync(ProjectId projectId, string fromRef, string toRef, long? fromProjectId = null,
        bool? straight = null, bool? unidiff = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabContributor> ListContributorsAsync(ProjectId projectId,
        ContributorListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabCommit> GetMergeBaseAsync(ProjectId projectId, IReadOnlyList<string> refs,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiffStat> ListDiffStatsAsync(ProjectId projectId, string fromRef, string toRef,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabChangedPath> ListChangedPathsAsync(ProjectId projectId, string fromRef, string toRef,
        bool? findRenames = null, CancellationToken cancellationToken = default);

    Task<GitLabDivergingCommitCount> GetDivergingCommitCountsAsync(ProjectId projectId, string fromRef, string toRef,
        int? maxCount = null, CancellationToken cancellationToken = default);

    Task<GitLabChangelog> GenerateChangelogAsync(ProjectId projectId, string version, string? fromRef = null,
        string? toRef = null, DateTimeOffset? releaseDate = null, string? trailer = null, string? configFile = null,
        string? configFileRef = null, CancellationToken cancellationToken = default);

    Task AddChangelogAsync(ProjectId projectId, AddChangelogRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId, RepositoryArchiveOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadSnapshotAsync(ProjectId projectId, bool? wiki = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBlob> GetBlobAsync(ProjectId projectId, string sha, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawBlobAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GitLabBatchBlob>> GetBlobsAsync(ProjectId projectId, BlobBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRepositoryHealth> GetHealthAsync(ProjectId projectId, bool? generate = null,
        CancellationToken cancellationToken = default);

    Task<GitLabCommit> UpdateSubmoduleAsync(ProjectId projectId, string submodule, UpdateSubmoduleRequest request,
        CancellationToken cancellationToken = default);
}