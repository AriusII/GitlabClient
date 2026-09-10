namespace GitLab.Client.Models;

/// <summary>
///     Added/deleted line counts for one file that differs between two refs
///     (<c>GET /projects/:id/repository/diff_stats</c>).
/// </summary>
public sealed record GitLabDiffStat
{
    public required string Path { get; init; }

    /// <summary>The path on the "from" side; differs from <see cref="Path" /> only for a rename.</summary>
    public string? OldPath { get; init; }

    public int? Additions { get; init; }

    public int? Deletions { get; init; }
}