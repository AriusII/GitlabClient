namespace GitLab.Client.Models;

/// <summary>
///     Body for <c>POST /projects/:id/export</c>. Every member is optional - an empty request schedules a
///     plain export you then poll for and download.
/// </summary>
public sealed record ExportProjectRequest
{
    /// <summary>Overrides the description recorded in the archive.</summary>
    public string? Description { get; init; }

    /// <summary>Push the finished archive somewhere instead of holding it for download.</summary>
    public GitLabProjectExportUpload? Upload { get; init; }

    /// <summary>
    ///     Relations to leave out of the archive, such as <c>["merge_requests", "issues"]</c>. Omit to export
    ///     everything.
    /// </summary>
    public IReadOnlyList<string>? ExcludedRelations { get; init; }
}