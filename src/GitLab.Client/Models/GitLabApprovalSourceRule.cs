namespace GitLab.Client.Models;

/// <summary>
///     The project-level rule a merge-request approval rule was copied from
///     (<c>APIEntitiesMergeRequestApprovalRuleSourceRule</c>). Only the approval count is exposed by GitLab.
/// </summary>
public sealed record GitLabApprovalSourceRule
{
    public int? ApprovalsRequired { get; init; }
}