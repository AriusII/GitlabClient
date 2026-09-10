namespace GitLab.Client.Models;

/// <summary>
///     A file attached to a group's markdown, as listed by <c>GET /groups/:id/uploads</c>. The bytes
///     themselves are fetched separately, through one of the download routes.
/// </summary>
public sealed record GitLabGroupUpload
{
    public required long Id { get; init; }

    /// <summary>Size in bytes.</summary>
    public long? Size { get; init; }

    public string? Filename { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Who uploaded the file. Null for files GitLab itself created, such as an imported avatar.</summary>
    public GitLabUser? UploadedBy { get; init; }
}