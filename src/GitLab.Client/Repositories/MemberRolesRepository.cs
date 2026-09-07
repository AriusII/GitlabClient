using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MemberRolesRepository(IGitLabApiConnection connection) : IMemberRolesRepository
{
    public IAsyncEnumerable<GitLabMemberRole> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("member_roles").Build(),
            GitLabJsonContext.Default.GitLabMemberRoleArray,
            cancellationToken);
    }

    public Task<GitLabMemberRole> CreateAsync(CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("member_roles").Build(),
            request,
            GitLabJsonContext.Default.CreateMemberRoleRequest,
            GitLabJsonContext.Default.GitLabMemberRole,
            cancellationToken);
    }

    public Task DeleteAsync(long memberRoleId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("member_roles").Segment(memberRoleId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMemberRole> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("member_roles").Build(),
            GitLabJsonContext.Default.GitLabMemberRoleArray,
            cancellationToken);
    }

    public Task<GitLabMemberRole> CreateForGroupAsync(GroupId groupId, CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("member_roles").Build(),
            request,
            GitLabJsonContext.Default.CreateMemberRoleRequest,
            GitLabJsonContext.Default.GitLabMemberRole,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, long memberRoleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("member_roles").Segment(memberRoleId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMemberRole> ListAdminRolesAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("admin_member_roles").Build(),
            GitLabJsonContext.Default.GitLabMemberRoleArray,
            cancellationToken);
    }

    public Task<GitLabMemberRole> CreateAdminRoleAsync(CreateAdminMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("admin_member_roles").Build(),
            request,
            GitLabJsonContext.Default.CreateAdminMemberRoleRequest,
            GitLabJsonContext.Default.GitLabMemberRole,
            cancellationToken);
    }

    public Task DeleteAdminRoleAsync(long memberRoleId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("admin_member_roles").Segment(memberRoleId).Build(),
            cancellationToken);
    }
}