using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Users resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IUsersService), typeof(IUsersClient))]
internal interface IUsersRepository
{
    Task<GitLabUser> GetAsync(long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListAsync(UserListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabUser> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<GitLabUserCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    Task<GitLabUser> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<GitLabUser> UpdateAsync(long userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long userId, bool? hardDelete = null, CancellationToken cancellationToken = default);

    Task ActivateAsync(long userId, CancellationToken cancellationToken = default);

    Task DeactivateAsync(long userId, CancellationToken cancellationToken = default);

    Task BlockAsync(long userId, CancellationToken cancellationToken = default);

    Task UnblockAsync(long userId, CancellationToken cancellationToken = default);

    Task BanAsync(long userId, CancellationToken cancellationToken = default);

    Task UnbanAsync(long userId, CancellationToken cancellationToken = default);

    Task ApproveAsync(long userId, CancellationToken cancellationToken = default);

    Task RejectAsync(long userId, CancellationToken cancellationToken = default);

    Task DisableTwoFactorAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabUserAssociationsCount> GetAssociationsCountAsync(long userId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabEmail> ListEmailsAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabEmail> AddEmailAsync(long userId, AddUserEmailRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteEmailAsync(long userId, long emailId, CancellationToken cancellationToken = default);

    Task DeleteIdentityAsync(long userId, string provider, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUserMembership> ListMembershipsAsync(long userId,
        UserMembershipListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabUser> FollowAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabUser> UnfollowAsync(long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListFollowersAsync(long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListFollowingAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabUserStatus> GetStatusAsync(string userIdOrUsername, CancellationToken cancellationToken = default);

    Task<GitLabUserSupportPin> GetSupportPinAsync(long userId, CancellationToken cancellationToken = default);

    Task RevokeSupportPinAsync(long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListEnterpriseUsersAsync(GroupId groupId, EnterpriseUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabUser> GetEnterpriseUserAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabUser> UpdateEnterpriseUserAsync(GroupId groupId, long userId, UpdateEnterpriseUserRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteEnterpriseUserAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    Task DisableEnterpriseUserTwoFactorAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);
}