using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's LDAP API: it searches configured instance directories and manages the LDAP group
///     links that synchronise directory membership into a GitLab group.
///     <para>
///         LDAP settings and the directory itself are instance administrator concerns. Managing a group's
///         links or synchronising them additionally requires administrator or Owner access to that group;
///         unavailable LDAP features surface as GitLab's regular authorization or feature-tier errors.
///     </para>
/// </summary>
public interface ILdapClient
{
    /// <summary>
    ///     Streams LDAP groups visible through every configured provider. GitLab limits this directory
    ///     search to twenty results; <paramref name="search" /> narrows the result by common name.
    /// </summary>
    IAsyncEnumerable<GitLabLdapGroup> ListGroupsAsync(string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams LDAP groups visible through one configured provider. The provider name is path-escaped,
    ///     and <paramref name="search" /> narrows the result by common name.
    /// </summary>
    IAsyncEnumerable<GitLabLdapGroup> ListGroupsForProviderAsync(string provider, string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every LDAP directory-to-GitLab-group link configured on <paramref name="groupId" />.</summary>
    IAsyncEnumerable<GitLabGroupLdapLink> ListGroupLinksAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an LDAP group link and grants its matching directory members the requested GitLab role.
    ///     A link may select a directory group by CN or by LDAP filter; see
    ///     <see cref="CreateLdapGroupLinkRequest" /> for the required selection rules.
    /// </summary>
    Task<GitLabGroupLdapLink> CreateGroupLinkAsync(GroupId groupId, CreateLdapGroupLinkRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes one LDAP group link, selected by a required provider plus a CN or filter. The two legacy
    ///     CN-in-path deletion endpoints are intentionally not exposed because GitLab has deprecated them.
    /// </summary>
    Task DeleteGroupLinkAsync(GroupId groupId, DeleteLdapGroupLinkOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts an immediate LDAP synchronisation for a group. GitLab performs the directory work
    ///     asynchronously after accepting this request; observe the group's member list for the result.
    /// </summary>
    Task SynchronizeGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}