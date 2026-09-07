using System.Runtime.CompilerServices;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Repositories;

namespace GitLab.Client.Tests.TestSupport;

internal sealed class FakeUsersRepository : IUsersRepository
{
    public Func<long, CancellationToken, Task<GitLabUser>>? OnGetAsync { get; set; }

    public Func<UserListOptions?, CancellationToken, IAsyncEnumerable<GitLabUser>>? OnListAsync { get; set; }

    public Func<CancellationToken, Task<GitLabUser>>? OnGetCurrentAsync { get; set; }

    public Task<GitLabUser> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        return (OnGetAsync ?? throw new InvalidOperationException($"{nameof(OnGetAsync)} was not configured."))(userId,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListAsync(UserListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return (OnListAsync ?? throw new InvalidOperationException($"{nameof(OnListAsync)} was not configured."))(
            options, cancellationToken);
    }

    public Task<GitLabUser> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return (OnGetCurrentAsync ??
                throw new InvalidOperationException($"{nameof(OnGetCurrentAsync)} was not configured."))(
            cancellationToken);
    }

    // The Users resource grew well past the three members these fakes were written for. The generated
    // Service/Controller forwarders are what the tests here exercise, and they only ever touch the
    // member under test, so the rest of the surface is implemented as an explicit "not configured"
    // throw rather than another twenty unused Func properties.

    public Task<GitLabUserCounts> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> UpdateAsync(long userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DeleteAsync(long userId, bool? hardDelete = null, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task ActivateAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DeactivateAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task BlockAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task UnblockAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task BanAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task UnbanAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task ApproveAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task RejectAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DisableTwoFactorAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUserAssociationsCount> GetAssociationsCountAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public IAsyncEnumerable<GitLabEmail> ListEmailsAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabEmail> AddEmailAsync(long userId, AddUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DeleteEmailAsync(long userId, long emailId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DeleteIdentityAsync(long userId, string provider, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public IAsyncEnumerable<GitLabUserMembership> ListMembershipsAsync(long userId,
        UserMembershipListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> FollowAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> UnfollowAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public IAsyncEnumerable<GitLabUser> ListFollowersAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public IAsyncEnumerable<GitLabUser> ListFollowingAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUserStatus> GetStatusAsync(string userIdOrUsername,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUserSupportPin> GetSupportPinAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task RevokeSupportPinAsync(long userId, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public IAsyncEnumerable<GitLabUser> ListEnterpriseUsersAsync(GroupId groupId,
        EnterpriseUserListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> GetEnterpriseUserAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task<GitLabUser> UpdateEnterpriseUserAsync(GroupId groupId, long userId,
        UpdateEnterpriseUserRequest request, CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DeleteEnterpriseUserAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    public Task DisableEnterpriseUserTwoFactorAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        throw NotConfigured();
    }

    private static InvalidOperationException NotConfigured(
        [CallerMemberName] string? member = null)
    {
        return new InvalidOperationException($"{member} was not configured.");
    }
}