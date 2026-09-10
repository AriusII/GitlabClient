namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /groups/:id/approval_rules/:approval_rule_id</c>.</summary>
/// <remarks>
///     An approver list sent to GitLab is authoritative. Send only the users and groups that should remain on the
///     rule; leave both lists unset to keep the existing approvers.
/// </remarks>
public sealed record UpdateGroupApprovalRuleRequest
{
    public string? Name { get; init; }

    public int? ApprovalsRequired { get; init; }

    /// <summary>
    ///     Optional, and best left unset: GitLab's own documentation says not to send <c>rule_type</c> when
    ///     building approval rules through the API.
    /// </summary>
    public GitLabApprovalRuleType? RuleType { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }
}