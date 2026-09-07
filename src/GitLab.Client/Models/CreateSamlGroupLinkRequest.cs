namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /groups/:id/saml_group_links</c>.</summary>
public sealed record CreateSamlGroupLinkRequest
{
    /// <summary>
    ///     The name of the SAML group to link, as it appears in the SAML assertion. GitLab echoes it back
    ///     as <see cref="GitLabSamlGroupLink.Name" />.
    /// </summary>
    public required string SamlGroupName { get; init; }

    /// <summary>
    ///     The role to grant members of the SAML group. GitLab accepts 5 (Minimal Access), 10 (Guest),
    ///     15 (Planner), 20 (Reporter), 25, 30 (Developer), 40 (Maintainer) and 50 (Owner); left an
    ///     <see cref="int" /> for the same reason the rest of the library does, since the accepted set
    ///     grows with new roles.
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>An optional custom member role to grant alongside <see cref="AccessLevel" />.</summary>
    public long? MemberRoleId { get; init; }

    /// <summary>
    ///     The SAML provider this link applies to. Required only on instances with more than one
    ///     configured provider.
    /// </summary>
    public string? Provider { get; init; }
}