namespace GitLab.Client.Models;

/// <summary>
///     One approver's approval of a merge request, as embedded in
///     <see cref="GitLabMergeRequestApprovals.ApprovedBy" /> (<c>APIEntitiesApprovals</c>).
/// </summary>
public sealed record GitLabApproval
{
    public GitLabUser? User { get; init; }

    public DateTimeOffset? ApprovedAt { get; init; }
}