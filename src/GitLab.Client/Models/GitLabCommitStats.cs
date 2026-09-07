namespace GitLab.Client.Models;

/// <summary>
///     The line counts GitLab attaches to a commit's detailed payload
///     (<c>POST /projects/:id/repository/commits</c>, <c>GET /projects/:id/repository/commits/:sha</c>).
///     Absent from the compact commit embedded in branches, tags and tree listings.
/// </summary>
public sealed record GitLabCommitStats
{
    /// <summary>Lines added across the commit.</summary>
    public int? Additions { get; init; }

    /// <summary>Lines removed across the commit.</summary>
    public int? Deletions { get; init; }

    /// <summary>Lines touched in total - <see cref="Additions" /> plus <see cref="Deletions" />.</summary>
    public int? Total { get; init; }
}