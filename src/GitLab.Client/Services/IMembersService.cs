using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Members, sitting between the public <c>IMembersClient</c>
///     controller and <c>IMembersRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IMembersService
{
    IAsyncEnumerable<GitLabMember> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabMember> GetAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    Task<GitLabMember> AddAsync(ProjectId projectId, AddMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> UpdateAsync(ProjectId projectId, long userId, UpdateMemberRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMember> ListIncludingInheritedForProjectAsync(ProjectId projectId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabMember> GetIncludingInheritedForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMember> ListForGroupAsync(GroupId groupId, GroupMemberListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> GetForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMember> ListIncludingInheritedForGroupAsync(GroupId groupId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabMember> GetIncludingInheritedForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> AddForGroupAsync(GroupId groupId, AddGroupMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> UpdateForGroupAsync(GroupId groupId, long userId, UpdateGroupMemberRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveForGroupAsync(GroupId groupId, long userId, RemoveGroupMemberOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPendingMember> ListPendingForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task ApproveForGroupAsync(GroupId groupId, long memberId, CancellationToken cancellationToken = default);

    Task ApproveAllForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task UpdateStateForGroupAsync(GroupId groupId, long userId, GitLabMembershipState state,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> SetOverrideForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    Task RemoveOverrideForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    Task RemoveBillableMemberForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBillableMembership> ListBillableMembershipsForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBillableMembership> ListIndirectBillableMembershipsForGroupAsync(GroupId groupId,
        long userId, CancellationToken cancellationToken = default);
}