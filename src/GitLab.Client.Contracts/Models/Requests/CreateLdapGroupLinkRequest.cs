namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /groups/:id/ldap_group_links</c>. Supply exactly one of
///     <see cref="Cn" /> and <see cref="Filter" />: a CN selects one directory group, while a filter can
///     match a dynamic directory membership. Filter links require GitLab Premium or Ultimate.
/// </summary>
public sealed record CreateLdapGroupLinkRequest
{
    /// <summary>The common name of one LDAP group. Mutually exclusive with <see cref="Filter" />.</summary>
    public string? Cn { get; init; }

    /// <summary>
    ///     The LDAP filter selecting one or more directory groups. Mutually exclusive with <see cref="Cn" />
    ///     and available in GitLab Premium and Ultimate only.
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    ///     The role granted to matching LDAP members: 5 Minimal Access, 10 Guest, 15 Planner, 20 Reporter,
    ///     25, 30 Developer, 40 Maintainer or 50 Owner.
    /// </summary>
    public required int GroupAccess { get; init; }

    /// <summary>The configured LDAP provider that resolves <see cref="Cn" /> or <see cref="Filter" />.</summary>
    public required string Provider { get; init; }

    /// <summary>An optional custom member role granted alongside <see cref="GroupAccess" />.</summary>
    public long? MemberRoleId { get; init; }
}