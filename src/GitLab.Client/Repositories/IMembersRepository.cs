using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Members resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMembersService), typeof(IMembersClient))]
internal interface IMembersRepository
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