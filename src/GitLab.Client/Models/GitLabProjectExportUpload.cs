namespace GitLab.Client.Models;

/// <summary>
///     Where GitLab should push the finished archive, instead of leaving it to be downloaded from
///     <c>/projects/:id/export/download</c>. Set on <see cref="ExportProjectRequest.Upload" />.
/// </summary>
public sealed record GitLabProjectExportUpload
{
    /// <summary>
    ///     The destination, typically a pre-signed URL on an S3-compatible store. Its query string may itself
    ///     carry credentials, so treat this value as a secret.
    /// </summary>
    public Uri? Url { get; init; }

    /// <summary>The verb to upload with. Defaults to <see cref="GitLabProjectExportUploadMethod.Put" /> server-side.</summary>
    public GitLabProjectExportUploadMethod? HttpMethod { get; init; }
}