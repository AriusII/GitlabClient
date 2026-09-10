using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Member roles" API area (<c>/member_roles</c>, <c>/groups/:id/member_roles</c> and
///     <c>/admin_member_roles</c>) - custom roles, which are one of GitLab's built-in roles plus a set of
///     individually granted abilities.
///     <para>
///         There are three separate collections, not one collection with a filter. A <b>group</b> member role
///         belongs to a top-level group and is the only form self-managed-plus-SaaS code can assume; an
///         <b>instance</b> member role exists only on self-managed and Dedicated instances; an <b>admin</b>
///         role carries no base access level and only the <c>read_admin_*</c> abilities, which is why it takes
///         <see cref="CreateAdminMemberRoleRequest" /> rather than <see cref="CreateMemberRoleRequest" />.
///     </para>
///     <para>
///         Roles are created and deleted, never updated - GitLab publishes no <c>PUT</c>/<c>PATCH</c> route
///         here. Deleting a role that is still assigned to a member fails with a
///         <see cref="Exceptions.GitLabValidationException" />.
///     </para>
///     <para>Custom roles are an Ultimate feature; a lower tier answers <c>403</c>.</para>
/// </summary>
public interface IMemberRolesClient
{
    /// <summary>
    ///     Streams every instance-level member role. Self-managed and GitLab Dedicated only; requires instance
    ///     administrator rights.
    /// </summary>
    IAsyncEnumerable<GitLabMemberRole> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an instance-level member role.
    ///     <see cref="CreateMemberRoleRequest.BaseAccessLevel" /> picks the built-in role the abilities are
    ///     layered on top of.
    /// </summary>
    Task<GitLabMemberRole> CreateAsync(CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an instance-level member role by its id.</summary>
    Task DeleteAsync(long memberRoleId, CancellationToken cancellationToken = default);

    /// <summary>Streams every member role defined on a group.</summary>
    IAsyncEnumerable<GitLabMemberRole> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a member role to a group. GitLab only accepts this on the root of a group hierarchy, so a
    ///     subgroup answers <c>400</c>.
    /// </summary>
    Task<GitLabMemberRole> CreateForGroupAsync(GroupId groupId, CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a member role from a group by its id.</summary>
    Task DeleteForGroupAsync(GroupId groupId, long memberRoleId, CancellationToken cancellationToken = default);

    /// <summary>Streams every custom admin role on the instance.</summary>
    IAsyncEnumerable<GitLabMemberRole> ListAdminRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a custom admin role. An admin role has no base access level - it only grants the
    ///     <c>read_admin_*</c> abilities in <see cref="CreateAdminMemberRoleRequest" />.
    /// </summary>
    Task<GitLabMemberRole> CreateAdminRoleAsync(CreateAdminMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a custom admin role by its id.</summary>
    Task DeleteAdminRoleAsync(long memberRoleId, CancellationToken cancellationToken = default);
}