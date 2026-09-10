namespace GitLab.Client.Models;

/// <summary>
///     The <c>_links</c> object on a completed project export - where the archive can be fetched from.
///     It remains available while <see cref="GitLabProjectExportStatus.ExportStatus" /> is
///     <see cref="GitLabProjectExportState.RegenerationInProgress" />.
/// </summary>
public sealed record GitLabProjectExportLinks
{
    /// <summary>The API route that streams the archive (<c>/projects/:id/export/download</c>).</summary>
    public Uri? ApiUrl { get; init; }

    /// <summary>The web UI route that downloads the archive.</summary>
    public Uri? WebUrl { get; init; }
}