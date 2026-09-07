namespace GitLab.Client.Models;

/// <summary>
///     One path that differs between two refs, with its change status and file modes
///     (<c>GET /projects/:id/repository/changed_paths</c>).
/// </summary>
public sealed record GitLabChangedPath
{
    public required string Path { get; init; }

    /// <summary>GitLab's change status - "added", "modified", "deleted", "renamed", "copied", "type_change".</summary>
    public string? Status { get; init; }

    public string? OldPath { get; init; }

    public string? OldMode { get; init; }

    public string? NewMode { get; init; }
}