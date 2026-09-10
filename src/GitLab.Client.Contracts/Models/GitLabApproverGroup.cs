namespace GitLab.Client.Models;

/// <summary>
///     One approver group of a project's approval configuration (<c>APIEntitiesApproverGroup</c>, embedded in
///     <see cref="GitLabProjectApprovalConfiguration.ApproverGroups" />). GitLab wraps the group in a
///     single-member object rather than projecting it directly.
/// </summary>
public sealed record GitLabApproverGroup
{
    public GitLabGroup? Group { get; init; }
}