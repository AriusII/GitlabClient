using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Repository files" API area (<c>/projects/:id/repository/files</c>).</summary>
public interface IRepositoryFilesClient
{
    Task<GitLabRepositoryFile> GetAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);

    Task<GitLabRepositoryFile> CreateAsync(ProjectId projectId, string filePath, CreateRepositoryFileRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRepositoryFile> UpdateAsync(ProjectId projectId, string filePath, UpdateRepositoryFileRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string filePath, string branch, string commitMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a file's metadata without transferring its contents
    ///     (<c>HEAD /projects/:id/repository/files/:file_path</c>).
    /// </summary>
    /// <remarks>
    ///     GitLab puts the answer in headers rather than a body, so read it back through
    ///     <see cref="GitLabHeadResponse.GetHeaderValue(string)" />: <c>X-Gitlab-Blob-Id</c>,
    ///     <c>X-Gitlab-Size</c>, <c>X-Gitlab-Encoding</c>, <c>X-Gitlab-File-Name</c>,
    ///     <c>X-Gitlab-File-Path</c>, <c>X-Gitlab-Ref</c>, <c>X-Gitlab-Commit-Id</c>,
    ///     <c>X-Gitlab-Last-Commit-Id</c>, <c>X-Gitlab-Content-Sha256</c> and
    ///     <c>X-Gitlab-Execute-Filemode</c>. A missing file comes back as
    ///     <see cref="GitLabHeadResponse.Exists" /> being <see langword="false" /> rather than as a
    ///     <see cref="Exceptions.GitLabNotFoundException" />.
    /// </remarks>
    /// <param name="projectId">The project that owns the file.</param>
    /// <param name="filePath">Full path from the repository root. Slashes are URL-encoded for you.</param>
    /// <param name="refName">The branch, tag or commit SHA to read at.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The probe result and its headers.</returns>
    Task<GitLabHeadResponse> GetMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a file's raw contents (<c>GET /projects/:id/repository/files/:file_path/raw</c>),
    ///     bypassing the Base64 round trip <see cref="GetAsync" /> goes through.
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="GitLabFileResponse" /> owns the open HTTP response and must be disposed
    ///     by the caller - <c>await using</c> it - or the pooled connection is never returned.
    /// </remarks>
    /// <param name="projectId">The project that owns the file.</param>
    /// <param name="filePath">Full path from the repository root. Slashes are URL-encoded for you.</param>
    /// <param name="refName">The branch, tag or commit SHA to read at. Defaults to the default branch.</param>
    /// <param name="lfs">Resolves an LFS pointer to the real object instead of returning the pointer file.</param>
    /// <param name="cancellationToken">Cancels the request and any read from the returned stream.</param>
    /// <returns>The open body, which the caller owns and must dispose.</returns>
    Task<GitLabFileResponse> GetRawAsync(ProjectId projectId, string filePath, string? refName = null,
        bool? lfs = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a file's blame, one entry per run of lines sharing a commit
    ///     (<c>GET /projects/:id/repository/files/:file_path/blame</c>).
    /// </summary>
    /// <param name="projectId">The project that owns the file.</param>
    /// <param name="filePath">Full path from the repository root. Slashes are URL-encoded for you.</param>
    /// <param name="refName">The branch, tag or commit SHA to blame at.</param>
    /// <param name="rangeStart">First line to blame. Supply it together with <paramref name="rangeEnd" />.</param>
    /// <param name="rangeEnd">Last line to blame.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The blame ranges, in file order.</returns>
    IAsyncEnumerable<GitLabBlameRange> GetBlameAsync(ProjectId projectId, string filePath, string refName,
        int? rangeStart = null, int? rangeEnd = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the blame's metadata without transferring it
    ///     (<c>HEAD /projects/:id/repository/files/:file_path/blame</c>). Carries the same
    ///     <c>X-Gitlab-*</c> headers as <see cref="GetMetadataAsync" />.
    /// </summary>
    Task<GitLabHeadResponse> GetBlameMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);
}