namespace GitLab.Client.Models;

/// <summary>
///     A link from a group in the SAML identity provider to a role in a GitLab group, as returned by
///     <c>/groups/:id/saml_group_links</c>. Members of the named SAML group are granted
///     <see cref="AccessLevel" /> in the GitLab group when they sign in through SAML.
/// </summary>
public sealed record GitLabSamlGroupLink
{
    /// <summary>
    ///     The name of the SAML group, as it appears in the SAML assertion's groups attribute. Note that
    ///     the create request calls this field <c>saml_group_name</c>; GitLab returns it as <c>name</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The role granted to members of the SAML group: 5 (Minimal Access), 10 (Guest), 15 (Planner),
    ///     20 (Reporter), 25, 30 (Developer), 40 (Maintainer) or 50 (Owner).
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>The custom member role granted alongside <see cref="AccessLevel" />, when one is configured.</summary>
    public long? MemberRoleId { get; init; }

    /// <summary>
    ///     The SAML provider this link applies to. Only meaningful on instances with more than one
    ///     configured provider, where it is also what disambiguates two links sharing a
    ///     <see cref="Name" />.
    /// </summary>
    public string? Provider { get; init; }
}