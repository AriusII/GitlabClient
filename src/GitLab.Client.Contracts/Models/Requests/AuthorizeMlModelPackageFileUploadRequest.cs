namespace GitLab.Client.Models.Requests;

/// <summary>
///     The optional body of the Workhorse authorize step that precedes an ml_model package file upload
///     (<c>PUT .../packages/ml_models/:model_version_id/files/.../:file_name/authorize</c>). Mirrors
///     <see cref="AuthorizeGenericPackageFileUploadRequest" />'s shape - GitLab's package registry
///     upload-authorize step is the same two-state gate for every package format - but stays its own
///     type so each resource's public surface documents itself.
/// </summary>
public sealed record AuthorizeMlModelPackageFileUploadRequest
{
    /// <summary>The publication status to assign the file once it is uploaded.</summary>
    public GitLabPackageFileStatus? Status { get; init; }
}