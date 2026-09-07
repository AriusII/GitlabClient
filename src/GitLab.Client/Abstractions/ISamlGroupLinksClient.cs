using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "SAML group links" API area (<c>/groups/:id/saml_group_links</c>) - the rules
///     that turn membership of a group in the SAML identity provider into a role in a GitLab group.
///     <para>
///         A link is keyed by the SAML group's name, optionally narrowed by
///         <c>provider</c>. On an instance with more than one SAML provider two links can share a name,
///         and GitLab then answers a name-only lookup or delete with <c>422</c> asking for the provider
///         - which surfaces here as a <see cref="Exceptions.GitLabValidationException" />, not a
///         <see cref="Exceptions.GitLabNotFoundException" />.
///     </para>
///     <para>SAML group links are an Ultimate feature on a top-level group; elsewhere GitLab answers <c>404</c>.</para>
/// </summary>
public interface ISamlGroupLinksClient
{
    /// <summary>Streams every SAML group link configured on a group.</summary>
    IAsyncEnumerable<GitLabSamlGroupLink> ListAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one SAML group link by SAML group name. SAML group names are free text and routinely
    ///     contain <c>/</c> and spaces (they are often LDAP distinguished names); pass the name raw, the
    ///     route builder percent-encodes it. Supply <paramref name="provider" /> when the instance has
    ///     more than one SAML provider.
    /// </summary>
    Task<GitLabSamlGroupLink> GetAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a SAML group link, granting members of the named SAML group a role in the GitLab
    ///     group.
    /// </summary>
    Task<GitLabSamlGroupLink> CreateAsync(GroupId groupId, CreateSamlGroupLinkRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a SAML group link. Existing members keep their access until their next SAML sign-in,
    ///     when the link no longer re-grants it.
    /// </summary>
    Task DeleteAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);
}