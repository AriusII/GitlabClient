namespace GitLab.Client.Models;

/// <summary>
///     One individual approver of a project's approval configuration
///     (<c>APIEntitiesApprover</c>, embedded in <see cref="GitLabProjectApprovalConfiguration.Approvers" />).
///     GitLab wraps the user in a single-member object rather than projecting it directly.
/// </summary>
public sealed record GitLabApprover
{
    public GitLabUser? User { get; init; }
}