namespace GitLab.Client.Models;

/// <summary>
///     Instance-wide entity counts, as returned by <c>GET /application/statistics</c>. Administrators
///     only.
/// </summary>
public sealed record GitLabApplicationStatistics
{
    public int? Forks { get; init; }

    public int? Issues { get; init; }

    public int? MergeRequests { get; init; }

    public int? Notes { get; init; }

    public int? Snippets { get; init; }

    public int? SshKeys { get; init; }

    public int? Milestones { get; init; }

    public int? Users { get; init; }

    public int? Projects { get; init; }

    public int? Groups { get; init; }

    public int? ActiveUsers { get; init; }
}