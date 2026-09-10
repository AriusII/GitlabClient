namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PUT /projects/:id/packages/debian/:file_name/authorize</c> - the pre-flight check GitLab
///     Workhorse expects before the actual upload, naming which distribution/component the file that
///     follows will land in.
/// </summary>
public sealed record AuthorizeDebianPackageUploadRequest
{
    /// <summary>The Debian component the uploaded file belongs to. Required by the endpoint.</summary>
    public required string Component { get; init; }

    /// <summary>The Debian codename or suite the uploaded file targets.</summary>
    public string? Distribution { get; init; }
}