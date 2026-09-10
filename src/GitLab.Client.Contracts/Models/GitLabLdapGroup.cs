namespace GitLab.Client.Models;

/// <summary>
///     An LDAP group that is available from an instance directory, as returned by
///     <c>GET /ldap/groups</c> and <c>GET /ldap/:provider/groups</c>.
/// </summary>
/// <remarks>
///     This is a directory-search result, not a configured GitLab group link. To grant its members
///     access to a GitLab group, use <see cref="Requests.CreateLdapGroupLinkRequest" /> and the
///     <c>/groups/:id/ldap_group_links</c> route.
/// </remarks>
public sealed record GitLabLdapGroup
{
    /// <summary>The directory group's distinguished common name.</summary>
    public required string Cn { get; init; }
}