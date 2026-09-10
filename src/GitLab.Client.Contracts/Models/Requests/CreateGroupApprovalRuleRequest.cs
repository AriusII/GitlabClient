namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /groups/:id/approval_rules</c>.</summary>
/// <remarks>
///     GitLab deliberately gives group rules a smaller surface than project rules. Use this type instead of
///     <see cref="CreateApprovalRuleRequest" /> to make sending a project-only setting impossible.
/// </remarks>
public sealed record CreateGroupApprovalRuleRequest
{
    public required string Name { get; init; }

    /// <summary>Zero is legitimate - it is exactly what an <c>any_approver</c> rule uses.</summary>
    public required int ApprovalsRequired { get; init; }

    /// <summary>
    ///     Optional, and best left unset: GitLab's own documentation says not to send <c>rule_type</c> when
    ///     building approval rules through the API.
    /// </summary>
    public GitLabApprovalRuleType? RuleType { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }
}