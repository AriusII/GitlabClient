using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MembersRepository(IGitLabApiConnection connection) : IMembersRepository
{
    public IAsyncEnumerable<GitLabMember> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("members").Build(),
            GitLabJsonContext.Default.GitLabMemberArray,
            cancellationToken);
    }

    public Task<GitLabMember> GetAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("members").Segment(userId).Build(),
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task<GitLabMember> AddAsync(ProjectId projectId, AddMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("members").Build(),
            request,
            GitLabJsonContext.Default.AddMemberRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task<GitLabMember> UpdateAsync(ProjectId projectId, long userId, UpdateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("members").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateMemberRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task RemoveAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("members").Segment(userId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMember> ListIncludingInheritedForProjectAsync(ProjectId projectId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("members")
                .Literal("all")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMemberArray,
            cancellationToken);
    }

    public Task<GitLabMember> GetIncludingInheritedForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("members")
                .Literal("all")
                .Segment(userId)
                .Build(),
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMember> ListForGroupAsync(GroupId groupId, GroupMemberListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("members").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabMemberArray,
            cancellationToken);
    }

    public Task<GitLabMember> GetForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("members").Segment(userId).Build(),
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMember> ListIncludingInheritedForGroupAsync(GroupId groupId,
        AllMemberListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Literal("all")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMemberArray,
            cancellationToken);
    }

    public Task<GitLabMember> GetIncludingInheritedForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Literal("all")
                .Segment(userId)
                .Build(),
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task<GitLabMember> AddForGroupAsync(GroupId groupId, AddGroupMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("members").Build(),
            request,
            GitLabJsonContext.Default.AddGroupMemberRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task<GitLabMember> UpdateForGroupAsync(GroupId groupId, long userId, UpdateGroupMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("members").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupMemberRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task RemoveForGroupAsync(GroupId groupId, long userId, RemoveGroupMemberOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Segment(userId)
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPendingMember> ListPendingForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("pending_members").Build(),
            GitLabJsonContext.Default.GitLabPendingMemberArray,
            cancellationToken);
    }

    public Task ApproveForGroupAsync(GroupId groupId, long memberId, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Segment(memberId)
                .Literal("approve")
                .Build(),
            cancellationToken);
    }

    public Task ApproveAllForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("members").Literal("approve_all").Build(),
            cancellationToken);
    }

    public Task UpdateStateForGroupAsync(GroupId groupId, long userId, GitLabMembershipState state,
        CancellationToken cancellationToken = default)
    {
        // The state travels as a query parameter rather than a JSON body: GitLab runs Grape, which reads
        // the parameter from either place (its own documentation uses the query string), and the
        // connection has no PUT-with-body-and-no-response overload to model the bodiless 200 this
        // endpoint answers with. ToApiValue is generated from the enum's [JsonStringEnumMemberName]
        // values, so the query spelling provably cannot drift from the JSON one.
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Segment(userId)
                .Literal("state")
                .Query("state", state.ToApiValue())
                .Build(),
            cancellationToken);
    }

    public Task<GitLabMember> SetOverrideForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        // An empty-bodied action endpoint that answers with the updated member.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Segment(userId)
                .Literal("override")
                .Build(),
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task RemoveOverrideForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        // GitLab echoes the member back with the override cleared, but DELETE has no response-typed
        // overload on IGitLabApiConnection, so that body is discarded. Re-read the member with
        // GetForGroupAsync when the post-state matters.
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("members")
                .Segment(userId)
                .Literal("override")
                .Build(),
            cancellationToken);
    }

    public Task RemoveBillableMemberForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("billable_members").Segment(userId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBillableMembership> ListBillableMembershipsForGroupAsync(GroupId groupId,
        long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("billable_members")
                .Segment(userId)
                .Literal("memberships")
                .Build(),
            GitLabJsonContext.Default.GitLabBillableMembershipArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBillableMembership> ListIndirectBillableMembershipsForGroupAsync(GroupId groupId,
        long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("billable_members")
                .Segment(userId)
                .Literal("indirect")
                .Build(),
            GitLabJsonContext.Default.GitLabBillableMembershipArray,
            cancellationToken);
    }
}