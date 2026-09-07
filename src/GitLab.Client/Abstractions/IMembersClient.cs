using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Members" API area (<c>/projects/:id/members</c>, <c>/groups/:id/members</c>) -
///     who belongs to a project or group, at what role, and until when.
///     <para>
///         Two distinctions run through this whole surface. <b>Direct versus inherited</b>: the plain
///         <c>/members</c> routes return only memberships held on the project or group itself, while the
///         <c>ListIncludingInherited*</c> and <c>GetIncludingInherited*</c> methods wrap
///         <c>/members/all</c>, which also reports memberships coming from ancestor groups and invited
///         groups (reduced to the highest access level the member holds). An empty direct listing
///         therefore does not mean nobody has access. <b>Project versus group</b>: GitLab exposes the two
///         under separate routes with different parameters, so they are separate methods here rather than
///         overloads - an overload set would be ambiguous at any call site passing a bare ID, since both
///         <see cref="ProjectId" /> and <see cref="GroupId" /> convert implicitly from
///         <see cref="long" /> and <see cref="string" />.
///     </para>
/// </summary>
public interface IMembersClient
{
    /// <summary>Streams the project's direct members. Inherited members are not included.</summary>
    IAsyncEnumerable<GitLabMember> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Reads one direct project member. Throws <c>GitLabNotFoundException</c> when the membership is inherited.</summary>
    Task<GitLabMember> GetAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a user to a project at the requested access level.</summary>
    Task<GitLabMember> AddAsync(ProjectId projectId, AddMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Changes a direct project member's access level or expiry date.</summary>
    Task<GitLabMember> UpdateAsync(ProjectId projectId, long userId, UpdateMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a direct member from a project.</summary>
    Task RemoveAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every member of a project the caller may see, including those inherited from ancestor
    ///     groups and invited groups (<c>GET /projects/:id/members/all</c>). A member holding a membership
    ///     in several places appears once, at the highest access level.
    /// </summary>
    IAsyncEnumerable<GitLabMember> ListIncludingInheritedForProjectAsync(ProjectId projectId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one member of a project, looking through inherited and invited-group memberships as well
    ///     as direct ones (<c>GET /projects/:id/members/all/:user_id</c>).
    /// </summary>
    Task<GitLabMember> GetIncludingInheritedForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the group's direct members (<c>GET /groups/:id/members</c>). Members inherited from an
    ///     ancestor group or an invited group are not included.
    /// </summary>
    IAsyncEnumerable<GitLabMember> ListForGroupAsync(GroupId groupId, GroupMemberListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one direct group member (<c>GET /groups/:id/members/:user_id</c>).</summary>
    Task<GitLabMember> GetForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every member of a group the caller may see, including inherited and invited-group
    ///     memberships (<c>GET /groups/:id/members/all</c>), each reduced to their highest access level.
    /// </summary>
    IAsyncEnumerable<GitLabMember> ListIncludingInheritedForGroupAsync(GroupId groupId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one member of a group, looking through inherited and invited-group memberships as well as
    ///     direct ones (<c>GET /groups/:id/members/all/:user_id</c>).
    /// </summary>
    Task<GitLabMember> GetIncludingInheritedForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a user to a group at the requested access level. The group route identifies the new member
    ///     by ID or by username, so exactly one of <see cref="AddGroupMemberRequest.UserId" /> and
    ///     <see cref="AddGroupMemberRequest.Username" /> must be set.
    /// </summary>
    Task<GitLabMember> AddForGroupAsync(GroupId groupId, AddGroupMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Changes a direct group member's access level, expiry date or custom member role.</summary>
    Task<GitLabMember> UpdateForGroupAsync(GroupId groupId, long userId, UpdateGroupMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a member from a group. By default the removal cascades to the group's subgroups and
    ///     projects; <see cref="RemoveGroupMemberOptions" /> narrows that and can unassign the user from
    ///     the group's issues and merge requests at the same time.
    /// </summary>
    Task RemoveForGroupAsync(GroupId groupId, long userId, RemoveGroupMemberOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the group's memberships that are awaiting approval, plus the people invited by email who
    ///     have no GitLab account yet (<c>GET /groups/:id/pending_members</c>). Top-level groups only -
    ///     GitLab rejects the call on a subgroup.
    /// </summary>
    IAsyncEnumerable<GitLabPendingMember> ListPendingForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves one membership that is awaiting approval. <paramref name="memberId" /> is the
    ///     membership's own ID as reported by <see cref="ListPendingForGroupAsync" />, not the user ID -
    ///     this is the one route in this area that does not take a user ID.
    /// </summary>
    Task ApproveForGroupAsync(GroupId groupId, long memberId, CancellationToken cancellationToken = default);

    /// <summary>Approves every membership of the group that is awaiting approval, in one call.</summary>
    Task ApproveAllForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves a user's memberships in the group between the awaiting and active states
    ///     (<c>PUT /groups/:id/members/:user_id/state</c>), which is how a seat is freed or reclaimed
    ///     without removing the member.
    /// </summary>
    Task UpdateStateForGroupAsync(GroupId groupId, long userId, GitLabMembershipState state,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Flags a member's access level as manually overridden, so the next LDAP synchronisation leaves it
    ///     alone. Answers with the updated member.
    /// </summary>
    Task<GitLabMember> SetOverrideForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears the LDAP override, handing the member's access level back to the next synchronisation.
    ///     GitLab echoes the updated member, but a DELETE carries no typed response here - read it back
    ///     with <see cref="GetForGroupAsync" /> if the resulting access level matters.
    /// </summary>
    Task RemoveOverrideForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a billable member from a top-level group, which drops every membership they hold
    ///     anywhere beneath it and frees their seat.
    /// </summary>
    Task RemoveBillableMemberForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the direct memberships behind one billable member's seat - which groups and projects
    ///     under the top-level group they are actually a member of.
    /// </summary>
    IAsyncEnumerable<GitLabBillableMembership> ListBillableMembershipsForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the indirect memberships behind one billable member's seat - the ones they hold through
    ///     an invited group rather than directly.
    /// </summary>
    IAsyncEnumerable<GitLabBillableMembership> ListIndirectBillableMembershipsForGroupAsync(GroupId groupId,
        long userId, CancellationToken cancellationToken = default);
}