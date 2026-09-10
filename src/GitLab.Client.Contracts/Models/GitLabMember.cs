namespace GitLab.Client.Models;

/// <summary>
///     A member of a project or a group, as returned by every route of the GitLab Members API. The
///     direct routes (<c>/members</c>), the inherited ones (<c>/members/all</c>) and the group override
///     and access-request approvals all answer with this one shape - GitLab's <c>Member</c> entity - so
///     there is deliberately no separate "inherited member" type.
///     <para>
///         Only <see cref="Id" />, <see cref="Username" />, <see cref="Name" />, <see cref="WebUrl" /> and
///         <see cref="AccessLevel" /> are always present. The rest depend on the route and on the
///         caller's permissions: <see cref="IsUsingSeat" /> arrives only when the request asked for seat
///         information, <see cref="Email" /> only for administrators, and <see cref="Override" /> only on
///         LDAP-synchronised groups.
///     </para>
/// </summary>
public sealed record GitLabMember
{
    public required long Id { get; init; }

    public required string Username { get; init; }

    public required string Name { get; init; }

    public string? State { get; init; }

    /// <summary>Whether the user's account is locked out.</summary>
    public bool? Locked { get; init; }

    /// <summary>The email the user chose to publish on their profile, which is not necessarily <see cref="Email" />.</summary>
    public string? PublicEmail { get; init; }

    /// <summary>The member's primary email address. Returned to administrators only.</summary>
    public string? Email { get; init; }

    /// <summary>
    ///     The member's administrator-defined custom attributes, the same shape as
    ///     <see cref="GitLabUser.CustomAttributes" />.
    /// </summary>
    public IReadOnlyList<GitLabCustomAttribute>? CustomAttributes { get; init; }

    public Uri? AvatarUrl { get; init; }

    /// <summary>Instance-relative avatar path (<c>/uploads/-/system/user/avatar/1/avatar.png</c>), not an absolute URL.</summary>
    public string? AvatarPath { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     The role on the numeric ladder - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner,
    ///     plus the newer 5 (Minimal Access), 15 (Planner) and 25 (Admin) rungs. Typed as a string in the
    ///     spec, but GitLab sends an integer and expects one back. On <c>/members/all</c> this is the
    ///     highest level the member holds across this source and its ancestors, not the direct one.
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>When the membership was created. Absent from some embeddings, hence nullable.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Who added the member. Null for memberships GitLab created itself, such as a project's owner.</summary>
    public GitLabUser? CreatedBy { get; init; }

    /// <summary>When the membership lapses. A plain date, not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }

    public bool? TwoFactorEnabled { get; init; }

    /// <summary>
    ///     The member's group-scoped SAML identity, when the request includes
    ///     <c>with_saml_identity=true</c> and the caller is allowed to view it.
    /// </summary>
    public GitLabUserIdentity? GroupSamlIdentity { get; init; }

    /// <summary>The member's group-scoped SCIM identity, when visible to the caller.</summary>
    public GitLabProviderIdentity? GroupScimIdentity { get; init; }

    /// <summary>
    ///     Whether this member consumes a billable seat. Returned only when the request asked for seat
    ///     information (<c>show_seat_info=true</c>).
    /// </summary>
    public bool? IsUsingSeat { get; init; }

    /// <summary>
    ///     Whether the member's access level was set manually on top of an LDAP synchronisation, and so
    ///     survives the next sync. Set through <c>POST /groups/:id/members/:user_id/override</c>.
    /// </summary>
    public bool? Override { get; init; }

    /// <summary>Whether the membership is active or still awaiting approval.</summary>
    public GitLabMembershipState? MembershipState { get; init; }

    /// <summary>The custom member role layered over <see cref="AccessLevel" />, when one is assigned.</summary>
    public GitLabMemberRole? MemberRole { get; init; }
}