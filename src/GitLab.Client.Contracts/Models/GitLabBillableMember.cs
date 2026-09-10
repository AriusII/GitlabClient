namespace GitLab.Client.Models;

/// <summary>
///     A user who consumes a billable seat in a top-level group, as listed by
///     <c>GET /groups/:id/billable_members</c>.
///     <para>
///         Deliberately not <see cref="GitLabMember" />: a billable member is a user plus billing
///         metadata (<see cref="MembershipType" />, <see cref="Removable" />, <see cref="IsLastOwner" />)
///         and carries no single access level, since the same user may hold several memberships across
///         the hierarchy. Those memberships are listed separately, by the Members API.
///     </para>
/// </summary>
public sealed record GitLabBillableMember
{
    public required long Id { get; init; }

    public required string Username { get; init; }

    public required string Name { get; init; }

    public string? State { get; init; }

    public Uri? AvatarUrl { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>The member's primary email address. Returned to group owners and administrators only.</summary>
    public string? Email { get; init; }

    /// <summary>The email the user chose to publish on their profile, which is not necessarily <see cref="Email" />.</summary>
    public string? PublicEmail { get; init; }

    /// <summary>Last day the user was active anywhere on the instance. A plain date, not a timestamp.</summary>
    public DateOnly? LastActivityOn { get; init; }

    /// <summary>How the seat is held - <c>group_member</c>, <c>project_member</c> or <c>group_invite</c>.</summary>
    public string? MembershipType { get; init; }

    /// <summary>Whether this member can be removed from the group through the billable-members endpoint.</summary>
    public bool? Removable { get; init; }

    /// <summary>Whether removing this member would leave the group without an owner.</summary>
    public bool? IsLastOwner { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }

    public bool? TwoFactorEnabled { get; init; }

    /// <summary>Whether the membership is active or still awaiting approval.</summary>
    public string? MembershipState { get; init; }
}