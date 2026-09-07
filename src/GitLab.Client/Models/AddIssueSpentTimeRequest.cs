namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/issues/:issue_iid/add_spent_time</c>. The duration is added to
///     whatever is already logged; a negative duration subtracts from it.
/// </summary>
public sealed record AddIssueSpentTimeRequest
{
    /// <summary>The time to log, in the human format GitLab parses - <c>3h30m</c>, <c>-30m</c>.</summary>
    public required string Duration { get; init; }
}