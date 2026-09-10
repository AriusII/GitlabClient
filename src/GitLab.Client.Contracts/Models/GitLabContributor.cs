namespace GitLab.Client.Models;

/// <summary>
///     Per-author commit metrics for a repository
///     (<c>GET /projects/:id/repository/contributors</c>). Identity here is the Git author name and
///     email recorded in the commits, which is not necessarily a GitLab user.
/// </summary>
public sealed record GitLabContributor
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public int? Commits { get; init; }

    public int? Additions { get; init; }

    public int? Deletions { get; init; }
}