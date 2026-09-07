using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class UsersRepository(IGitLabApiConnection connection) : IUsersRepository
{
    public Task<GitLabUser> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListAsync(UserListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public Task<GitLabUser> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task<GitLabUserCounts> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user_counts").Build(),
            GitLabJsonContext.Default.GitLabUserCounts,
            cancellationToken);
    }

    public Task<GitLabUser> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Build(),
            request,
            GitLabJsonContext.Default.CreateUserRequest,
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task<GitLabUser> UpdateAsync(long userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateUserRequest,
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task DeleteAsync(long userId, bool? hardDelete = null, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Query("hard_delete", hardDelete).Build(),
            cancellationToken);
    }

    public Task ActivateAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("activate").Build(),
            cancellationToken);
    }

    public Task DeactivateAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("deactivate").Build(),
            cancellationToken);
    }

    public Task BlockAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("block").Build(),
            cancellationToken);
    }

    public Task UnblockAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("unblock").Build(),
            cancellationToken);
    }

    public Task BanAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("ban").Build(),
            cancellationToken);
    }

    public Task UnbanAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("unban").Build(),
            cancellationToken);
    }

    public Task ApproveAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("approve").Build(),
            cancellationToken);
    }

    public Task RejectAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("reject").Build(),
            cancellationToken);
    }

    public Task DisableTwoFactorAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("disable_two_factor").Build(),
            cancellationToken);
    }

    public Task<GitLabUserAssociationsCount> GetAssociationsCountAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("associations_count").Build(),
            GitLabJsonContext.Default.GitLabUserAssociationsCount,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEmail> ListEmailsAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("emails").Build(),
            GitLabJsonContext.Default.GitLabEmailArray,
            cancellationToken);
    }

    public Task<GitLabEmail> AddEmailAsync(long userId, AddUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("emails").Build(),
            request,
            GitLabJsonContext.Default.AddUserEmailRequest,
            GitLabJsonContext.Default.GitLabEmail,
            cancellationToken);
    }

    public Task DeleteEmailAsync(long userId, long emailId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("emails").Segment(emailId).Build(),
            cancellationToken);
    }

    public Task DeleteIdentityAsync(long userId, string provider, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("identities").Escaped(provider).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUserMembership> ListMembershipsAsync(long userId,
        UserMembershipListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("memberships").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabUserMembershipArray,
            cancellationToken);
    }

    public Task<GitLabUser> FollowAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("follow").Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task<GitLabUser> UnfollowAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("unfollow").Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListFollowersAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("followers").Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListFollowingAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("following").Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public Task<GitLabUserStatus> GetStatusAsync(string userIdOrUsername,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("users").Escaped(userIdOrUsername).Literal("status").Build(),
            GitLabJsonContext.Default.GitLabUserStatus,
            cancellationToken);
    }

    public Task<GitLabUserSupportPin> GetSupportPinAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("support_pin").Build(),
            GitLabJsonContext.Default.GitLabUserSupportPin,
            cancellationToken);
    }

    public Task RevokeSupportPinAsync(long userId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("support_pin").Literal("revoke").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListEnterpriseUsersAsync(GroupId groupId,
        EnterpriseUserListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("enterprise_users").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public Task<GitLabUser> GetEnterpriseUserAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("enterprise_users").Segment(userId).Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task<GitLabUser> UpdateEnterpriseUserAsync(GroupId groupId, long userId,
        UpdateEnterpriseUserRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("enterprise_users").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateEnterpriseUserRequest,
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task DeleteEnterpriseUserAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("enterprise_users").Segment(userId)
                .Query("hard_delete", hardDelete).Build(),
            cancellationToken);
    }

    public Task DisableEnterpriseUserTwoFactorAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("enterprise_users").Segment(userId)
                .Literal("disable_two_factor").Build(),
            cancellationToken);
    }
}