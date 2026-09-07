namespace GitLab.Client.Models;

/// <summary>
///     The badge counters GitLab shows in its own header for the authenticated user
///     (<c>GET /user_counts</c>). Every count is scoped to the caller, not to the instance.
/// </summary>
public sealed record GitLabUserCounts
{
    /// <summary>Legacy alias GitLab still sends for <see cref="AssignedMergeRequests" />.</summary>
    public int? MergeRequests { get; init; }

    public int? AssignedIssues { get; init; }

    public int? AssignedMergeRequests { get; init; }

    public int? ReviewRequestedMergeRequests { get; init; }

    /// <summary>Pending to-dos.</summary>
    public int? Todos { get; init; }
}