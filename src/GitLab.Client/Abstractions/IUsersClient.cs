using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Users" API area (<c>/users</c>, <c>/user</c>).
///     <para>
///         Two audiences share this resource and it is worth keeping them apart. The <c>/users/:id</c>
///         routes act on <em>another</em> account and most of them - create, update, delete, the
///         block/ban/deactivate/approve family, the email and identity sub-resources, memberships and the
///         support PIN - are administrators-only, answering <c>403 Forbidden</c>
///         (<see cref="Exceptions.GitLabForbiddenException" />) for anyone else. Only
///         <see cref="GetCurrentAsync" /> speaks for the token's own account.
///     </para>
/// </summary>
public interface IUsersClient
{
    /// <summary>Retrieves a single user by numeric ID (<c>GET /users/:id</c>).</summary>
    Task<GitLabUser> GetAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every user visible to the caller (<c>GET /users</c>), following GitLab's pagination.
    ///     Several filters on <paramref name="options" /> are honoured for administrators only.
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListAsync(UserListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves the account the configured token belongs to (<c>GET /user</c>).</summary>
    Task<GitLabUser> GetCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the caller's own badge counters - assigned issues, assigned and review-requested merge
    ///     requests, pending to-dos (<c>GET /user_counts</c>).
    /// </summary>
    Task<GitLabUserCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a user (<c>POST /users</c>). Administrators only.
    /// </summary>
    /// <param name="request">
    ///     The new account. It carries credentials-adjacent fields - never log it, and prefer
    ///     <see cref="CreateUserRequest.ResetPassword" /> over setting a password here.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The created user.</returns>
    /// <exception cref="Exceptions.GitLabValidationException">
    ///     GitLab rejected the account - a taken username or email, or no password strategy chosen.
    /// </exception>
    Task<GitLabUser> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a user (<c>PUT /users/:id</c>). Administrators only. Unset members of
    ///     <paramref name="request" /> are omitted, so this is a partial update despite the verb.
    /// </summary>
    Task<GitLabUser> UpdateAsync(long userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a user (<c>DELETE /users/:id</c>). Administrators only.
    /// </summary>
    /// <param name="userId">The account to delete.</param>
    /// <param name="hardDelete">
    ///     When <see langword="true" />, also deletes the contributions GitLab would otherwise move to the
    ///     Ghost User, and the groups the account solely owns. Check
    ///     <see cref="GetAssociationsCountAsync" /> first - this is not reversible.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task DeleteAsync(long userId, bool? hardDelete = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reactivates a deactivated account (<c>POST /users/:id/activate</c>). Administrators only. The
    ///     inverse of <see cref="DeactivateAsync" />; it does not lift a block or a ban.
    /// </summary>
    Task ActivateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deactivates a dormant account (<c>POST /users/:id/deactivate</c>). Administrators only. GitLab
    ///     accepts this only for accounts with no activity in the configured dormancy window, and unlike a
    ///     block the user can reactivate themselves by signing in.
    /// </summary>
    Task DeactivateAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Blocks an account (<c>POST /users/:id/block</c>). Administrators only. The user cannot sign in
    ///     or use the API, and their content stays visible.
    /// </summary>
    Task BlockAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Lifts a block (<c>POST /users/:id/unblock</c>). Administrators only.</summary>
    Task UnblockAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Bans an account (<c>POST /users/:id/ban</c>). Administrators only. Stronger than a block: the
    ///     user's issues, merge requests and comments are hidden as well.
    /// </summary>
    Task BanAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Lifts a ban and restores the account's content (<c>POST /users/:id/unban</c>). Administrators only.</summary>
    Task UnbanAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves a sign-up that is waiting on an administrator
    ///     (<c>POST /users/:id/approve</c>). Administrators only.
    /// </summary>
    Task ApproveAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rejects a pending sign-up and deletes the account (<c>POST /users/:id/reject</c>).
    ///     Administrators only. Valid only while the account is still awaiting approval.
    /// </summary>
    Task RejectAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Turns off two-factor authentication for an account that has lost its second factor
    ///     (<c>PATCH /users/:id/disable_two_factor</c>). Administrators only, and not permitted against
    ///     another administrator.
    /// </summary>
    Task DisableTwoFactorAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts what the account still owns - groups, projects, issues, merge requests
    ///     (<c>GET /users/:id/associations_count</c>). The check to make before
    ///     <see cref="DeleteAsync" /> with a hard delete.
    /// </summary>
    Task<GitLabUserAssociationsCount> GetAssociationsCountAsync(long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a user's secondary email addresses (<c>GET /users/:id/emails</c>). Administrators only.
    ///     The primary address is not among them; read it from <see cref="GitLabUser.Email" />.
    /// </summary>
    IAsyncEnumerable<GitLabEmail> ListEmailsAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a secondary email address to a user (<c>POST /users/:id/emails</c>). Administrators only.</summary>
    Task<GitLabEmail> AddEmailAsync(long userId, AddUserEmailRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a secondary email address (<c>DELETE /users/:id/emails/:email_id</c>). Administrators
    ///     only. GitLab refuses to remove the account's primary address.
    /// </summary>
    Task DeleteEmailAsync(long userId, long emailId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unlinks an external authentication identity
    ///     (<c>DELETE /users/:id/identities/:provider</c>). Administrators only.
    /// </summary>
    /// <param name="userId">The account to unlink the identity from.</param>
    /// <param name="provider">
    ///     The provider name, as it appears in <see cref="GitLabUserIdentity.Provider" /> - <c>ldapmain</c>,
    ///     <c>github</c>, <c>group_saml</c>.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task DeleteIdentityAsync(long userId, string provider, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every project and group a user belongs to
    ///     (<c>GET /users/:id/memberships</c>). Administrators only.
    /// </summary>
    IAsyncEnumerable<GitLabUserMembership> ListMembershipsAsync(long userId,
        UserMembershipListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Follows a user as the caller (<c>POST /users/:id/follow</c>), returning the followed account.</summary>
    Task<GitLabUser> FollowAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Stops following a user (<c>POST /users/:id/unfollow</c>), returning the unfollowed account.</summary>
    Task<GitLabUser> UnfollowAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Streams the accounts that follow this user (<c>GET /users/:id/followers</c>).</summary>
    IAsyncEnumerable<GitLabUser> ListFollowersAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Streams the accounts this user follows (<c>GET /users/:id/following</c>).</summary>
    IAsyncEnumerable<GitLabUser> ListFollowingAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a user's profile status - emoji, message and availability
    ///     (<c>GET /users/:id/status</c>).
    /// </summary>
    /// <param name="userIdOrUsername">
    ///     The numeric ID or the username. This is the one Users route GitLab accepts a username on, which
    ///     is why it takes a string rather than a <see cref="long" />.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabUserStatus> GetStatusAsync(string userIdOrUsername, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the account's current Support PIN (<c>GET /users/:id/support_pin</c>).
    ///     Administrators only. The PIN authenticates the user to GitLab Support - treat it as a credential.
    /// </summary>
    Task<GitLabUserSupportPin> GetSupportPinAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Invalidates the account's Support PIN (<c>POST /users/:id/support_pin/revoke</c>).
    ///     Administrators only.
    /// </summary>
    Task RevokeSupportPinAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the enterprise users of a group (<c>GET /groups/:id/enterprise_users</c>) - accounts the
    ///     group claims via SAML/SCIM provisioning, distinct from ordinary membership. Administrators and
    ///     group owners only.
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListEnterpriseUsersAsync(GroupId groupId, EnterpriseUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one of a group's enterprise users (<c>GET /groups/:id/enterprise_users/:user_id</c>).
    ///     Administrators and group owners only.
    /// </summary>
    Task<GitLabUser> GetEnterpriseUserAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates one of a group's enterprise users
    ///     (<c>PATCH /groups/:id/enterprise_users/:user_id</c>). Administrators and group owners only. Unset
    ///     members of <paramref name="request" /> are left unchanged.
    /// </summary>
    Task<GitLabUser> UpdateEnterpriseUserAsync(GroupId groupId, long userId, UpdateEnterpriseUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one of a group's enterprise users
    ///     (<c>DELETE /groups/:id/enterprise_users/:user_id</c>). Administrators and group owners only.
    /// </summary>
    /// <param name="groupId">The enterprise group the account is claimed by.</param>
    /// <param name="userId">The account to delete.</param>
    /// <param name="hardDelete">
    ///     When <see langword="true" />, also deletes the contributions GitLab would otherwise move to the
    ///     Ghost User, and the groups the account solely owns. Not reversible.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task DeleteEnterpriseUserAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Turns off two-factor authentication for one of a group's enterprise users
    ///     (<c>PATCH /groups/:id/enterprise_users/:user_id/disable_two_factor</c>). Administrators and group
    ///     owners only.
    /// </summary>
    Task DisableEnterpriseUserTwoFactorAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);
}