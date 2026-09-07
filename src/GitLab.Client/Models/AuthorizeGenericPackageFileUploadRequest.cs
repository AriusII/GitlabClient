namespace GitLab.Client.Models;

/// <summary>
///     The optional body of the Workhorse authorize step that precedes a generic package file upload
///     (<c>PUT .../packages/generic/:package_name/:package_version/:file_name/authorize</c>).
/// </summary>
public sealed record AuthorizeGenericPackageFileUploadRequest
{
    public GitLabPackageFileStatus? Status { get; init; }
}