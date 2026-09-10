namespace GitLab.Client.Models;

/// <summary>
///     One file belonging to a project package, as returned by
///     <c>GET /projects/:id/packages/:package_id/package_files</c> and, when a generic package upload asks
///     for it via <c>select=package_file</c>, by the upload endpoint itself.
/// </summary>
public sealed record GitLabPackageFile
{
    public required long Id { get; init; }

    public required long PackageId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public required string FileName { get; init; }

    public long? Size { get; init; }

    public string? FileMd5 { get; init; }

    public string? FileSha1 { get; init; }

    public string? FileSha256 { get; init; }

    /// <summary>
    ///     The pipeline that built this file, when GitLab can attribute one. Named <c>pipelines</c> on the
    ///     wire despite carrying a single object, not a list - reused from the Pipelines resource rather
    ///     than an invented duplicate, since every field this endpoint sends is already on
    ///     <see cref="GitLabPipeline" />.
    /// </summary>
    public GitLabPipeline? Pipelines { get; init; }
}