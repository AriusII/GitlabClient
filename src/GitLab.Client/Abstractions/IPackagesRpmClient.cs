using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's RPM package registry (<c>/projects/:id/packages/rpm/...</c>) - uploading a package
///     and downloading its files and the generated repository metadata that <c>yum</c>/<c>dnf</c> read.
///     <para>
///         The spec documents no multipart schema for the upload endpoint (unlike PyPI, RubyGems and
///         Helm, whose upload bodies are documented), so <see cref="UploadAsync" /> sends the package
///         under <see cref="GitLabFileUpload.DefaultFieldName" /> - the field name every other GitLab
///         package upload in this library uses, and the one <c>curl --form file=@pkg.rpm</c> examples in
///         GitLab's own documentation send it under.
///     </para>
/// </summary>
public interface IPackagesRpmClient
{
    /// <summary>
    ///     Uploads an RPM package. GitLab answers <c>201 Created</c> with no body, so this method
    ///     completes once the upload is accepted rather than returning a package DTO.
    /// </summary>
    Task UploadAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The workhorse pre-upload authorization check for <see cref="UploadAsync" />. Direct callers of
    ///     this client do not need it - it exists for workhorse-fronted deployments, not as a
    ///     precondition <see cref="UploadAsync" /> itself requires.
    /// </summary>
    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a generated repository metadata file (from the <c>repodata/</c> directory of the
    ///     project's RPM repository - <c>repomd.xml</c>, the primary/filelists/other XML, and so on).
    /// </summary>
    Task<GitLabFileResponse> DownloadRepositoryMetadataAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file of an uploaded RPM package by its package-file ID, as listed in the
    ///     repository metadata.
    /// </summary>
    Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageFileId, string fileName,
        CancellationToken cancellationToken = default);
}