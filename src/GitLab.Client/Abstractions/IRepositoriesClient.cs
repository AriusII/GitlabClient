using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Repositories" API area (<c>/projects/:id/repository</c>) - browsing the tree,
///     comparing refs, reading blobs, downloading archives, and the derived metrics GitLab computes from
///     the commit graph. The project snapshot (<c>/projects/:id/snapshot</c>) and the submodule pointer
///     update (<c>/projects/:id/repository/submodules/:submodule</c>) live here too: both are
///     repository-level operations GitLab files under tags of their own.
/// </summary>
public interface IRepositoriesClient
{
    /// <summary>Lists files and directories in the repository (<c>GET /projects/:id/repository/tree</c>).</summary>
    IAsyncEnumerable<GitLabTreeItem> ListTreeAsync(ProjectId projectId, TreeListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Compares two branches, tags or commits (<c>GET /projects/:id/repository/compare</c>).
    ///     <paramref name="straight" /> selects direct comparison (<c>from..to</c>) over the default
    ///     merge-base comparison (<c>from...to</c>).
    /// </summary>
    Task<GitLabCompare> CompareAsync(ProjectId projectId, string fromRef, string toRef, long? fromProjectId = null,
        bool? straight = null, bool? unidiff = null, CancellationToken cancellationToken = default);

    /// <summary>Per-author commit metrics (<c>GET /projects/:id/repository/contributors</c>).</summary>
    IAsyncEnumerable<GitLabContributor> ListContributorsAsync(ProjectId projectId,
        ContributorListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the common ancestor of two or more refs (<c>GET /projects/:id/repository/merge_base</c>).
    ///     GitLab answers 400 for fewer than two refs, so that is rejected here as an
    ///     <see cref="ArgumentOutOfRangeException" /> before the request is built.
    /// </summary>
    Task<GitLabCommit> GetMergeBaseAsync(ProjectId projectId, IReadOnlyList<string> refs,
        CancellationToken cancellationToken = default);

    /// <summary>Added/deleted line counts per changed file (<c>GET /projects/:id/repository/diff_stats</c>).</summary>
    IAsyncEnumerable<GitLabDiffStat> ListDiffStatsAsync(ProjectId projectId, string fromRef, string toRef,
        CancellationToken cancellationToken = default);

    /// <summary>Paths that differ between two refs (<c>GET /projects/:id/repository/changed_paths</c>).</summary>
    IAsyncEnumerable<GitLabChangedPath> ListChangedPathsAsync(ProjectId projectId, string fromRef, string toRef,
        bool? findRenames = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     How far two refs have diverged (<c>GET /projects/:id/repository/diverging_commits</c>).
    /// </summary>
    Task<GitLabDivergingCommitCount> GetDivergingCommitCountsAsync(ProjectId projectId, string fromRef, string toRef,
        int? maxCount = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates changelog markdown without committing it (<c>GET /projects/:id/repository/changelog</c>).
    ///     Use <see cref="AddChangelogAsync" /> to commit the same text to a file.
    /// </summary>
    Task<GitLabChangelog> GenerateChangelogAsync(ProjectId projectId, string version, string? fromRef = null,
        string? toRef = null, DateTimeOffset? releaseDate = null, string? trailer = null, string? configFile = null,
        string? configFileRef = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates changelog data and commits it to a file in the repository
    ///     (<c>POST /projects/:id/repository/changelog</c>). GitLab answers with no body, so this returns
    ///     nothing; read the committed file back through <see cref="IRepositoryFilesClient" /> if you need
    ///     the text.
    /// </summary>
    Task AddChangelogAsync(ProjectId projectId, AddChangelogRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the repository as a tar or zip archive
    ///     (<c>GET /projects/:id/repository/archive</c>). GitLab.com rate-limits this to five requests a
    ///     minute.
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="GitLabFileResponse" /> owns the open HTTP response and must be disposed
    ///     by the caller - <c>await using</c> it - or the pooled connection is never returned. GitLab names
    ///     the archive in <c>Content-Disposition</c>, which is surfaced as
    ///     <see cref="GitLabFileResponse.FileName" /> with any directory component stripped.
    /// </remarks>
    /// <param name="projectId">The project to archive.</param>
    /// <param name="options">Which ref, which format, and which subtree to archive.</param>
    /// <param name="cancellationToken">Cancels the request and any read from the returned stream.</param>
    /// <returns>The open archive body, which the caller owns and must dispose.</returns>
    Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId, RepositoryArchiveOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a Git snapshot of the project for disaster recovery
    ///     (<c>GET /projects/:id/snapshot</c>). Administrator-only.
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="GitLabFileResponse" /> owns the open HTTP response and must be disposed
    ///     by the caller - <c>await using</c> it.
    /// </remarks>
    /// <param name="projectId">The project to snapshot.</param>
    /// <param name="wiki">Snapshots the project's wiki repository instead of its code repository.</param>
    /// <param name="cancellationToken">Cancels the request and any read from the returned stream.</param>
    /// <returns>The open snapshot body, which the caller owns and must dispose.</returns>
    Task<GitLabFileResponse> DownloadSnapshotAsync(ProjectId projectId, bool? wiki = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a blob by its own SHA (<c>GET /projects/:id/repository/blobs/:sha</c>). The content comes
    ///     back Base64-encoded; <see cref="GetRawBlobAsync" /> streams the bytes instead.
    /// </summary>
    Task<GitLabBlob> GetBlobAsync(ProjectId projectId, string sha, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a blob's raw bytes (<c>GET /projects/:id/repository/blobs/:sha/raw</c>).
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="GitLabFileResponse" /> owns the open HTTP response and must be disposed
    ///     by the caller - <c>await using</c> it.
    /// </remarks>
    /// <param name="projectId">The project that owns the blob.</param>
    /// <param name="sha">The blob SHA.</param>
    /// <param name="cancellationToken">Cancels the request and any read from the returned stream.</param>
    /// <returns>The open blob body, which the caller owns and must dispose.</returns>
    Task<GitLabFileResponse> GetRawBlobAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads several files in one round trip (<c>POST /projects/:id/repository/blobs/batch</c>). GitLab
    ///     caps the batch at 20 files and truncates each blob at 1 MB, flagging that per entry. The result
    ///     is materialized rather than streamed: the endpoint answers with one unpaginated array.
    /// </summary>
    Task<IReadOnlyList<GitLabBatchBlob>> GetBlobsAsync(ProjectId projectId, BlobBatchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Storage statistics for the repository (<c>GET /projects/:id/repository/health</c>). Requires
    ///     push access; GitLab rate-limits report generation to five requests an hour per project.
    /// </summary>
    /// <param name="projectId">The project to report on.</param>
    /// <param name="generate">Recomputes the report instead of returning the cached one.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The storage statistics.</returns>
    Task<GitLabRepositoryHealth> GetHealthAsync(ProjectId projectId, bool? generate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Points a submodule at a different commit
    ///     (<c>PUT /projects/:id/repository/submodules/:submodule</c>) and returns the commit that recorded
    ///     the change. The submodule path is URL-encoded for you.
    /// </summary>
    Task<GitLabCommit> UpdateSubmoduleAsync(ProjectId projectId, string submodule, UpdateSubmoduleRequest request,
        CancellationToken cancellationToken = default);
}