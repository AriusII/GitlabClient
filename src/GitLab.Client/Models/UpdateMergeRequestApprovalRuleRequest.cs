namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/merge_requests/:iid/approval_rules/:approval_rule_id</c>.
///     <para>
///         Unset members are omitted rather than sent as null, so this is a partial update - with one
///         exception: GitLab treats the approver lists as authoritative and removes any approver or group not
///         named in <see cref="UserIds" />, <see cref="GroupIds" /> or <see cref="Usernames" />.
///     </para>
/// </summary>
public sealed record UpdateMergeRequestApprovalRuleRequest
{
    public string? Name { get; init; }

    public int? ApprovalsRequired { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }

    public IReadOnlyList<string>? Usernames { get; init; }

    public bool? RemoveHiddenGroups { get; init; }
}