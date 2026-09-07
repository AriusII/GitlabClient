using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Repositories resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IRepositoriesService), typeof(IRepositoriesClient))]
internal interface IRepositoriesRepository
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