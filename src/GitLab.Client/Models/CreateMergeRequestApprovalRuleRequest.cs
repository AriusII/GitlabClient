namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/merge_requests/:iid/approval_rules</c>.</summary>
public sealed record CreateMergeRequestApprovalRuleRequest
{
    public required string Name { get; init; }

    /// <summary>Zero is legitimate - it is exactly what an <c>any_approver</c> rule uses.</summary>
    public required int ApprovalsRequired { get; init; }

    /// <summary>The project-level approval rule this merge-request rule is copied from.</summary>
    public long? ApprovalProjectRuleId { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }

    public IReadOnlyList<string>? Usernames { get; init; }
}