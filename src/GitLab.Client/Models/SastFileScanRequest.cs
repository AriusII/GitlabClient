namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>POST /projects/:id/security_scans/sast/:sast_endpoint</c>: one file, scanned in
///     real time rather than through a pipeline.
/// </summary>
public sealed record SastFileScanRequest
{
    /// <summary>The project-relative path of the file being scanned. The scanner uses it to pick a parser.</summary>
    public required string FilePath { get; init; }

    /// <summary>The full text of the file to scan.</summary>
    public required string Content { get; init; }
}