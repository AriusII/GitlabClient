namespace GitLab.Client.Models;

/// <summary>
///     A branch or tag a commit has been pushed to
///     (<c>GET /projects/:id/repository/commits/:sha/refs</c>).
/// </summary>
public sealed record GitLabCommitRef
{
    /// <summary>GitLab's own wire value, <c>branch</c> or <c>tag</c>.</summary>
    public required string Type { get; init; }

    /// <summary>The branch or tag name.</summary>
    public required string Name { get; init; }
}