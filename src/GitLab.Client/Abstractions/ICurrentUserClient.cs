using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the part of the GitLab Users API that acts on <em>whoever the credential belongs to</em>
///     rather than on a user named in the route - <c>/user/emails</c>, <c>/user/status</c>,
///     <c>/user/preferences</c>, <c>/user/avatar</c>, <c>/user/support_pin</c>, <c>/user/runners</c> and
///     <c>/user/activities</c>.
///     <para>
///         There is no user id anywhere in this surface, by design: the token identifies the account, so
///         these calls follow the credential. Swapping the token swaps the account they operate on, which
///         is exactly what makes them the right choice in a CI job or a desktop tool and the wrong choice
///         in an administrative one - for acting on a specific user, use <c>IUsersClient</c>.
///     </para>
///     <para>
///         The neighbouring <c>/user/keys</c>, <c>/user/gpg_keys</c>, <c>/user/personal_access_tokens</c>
///         and <c>/user/applications</c> routes are the same idea and live on their own clients:
///         <c>ISshKeysClient</c>, <c>IGpgKeysClient</c>, <c>IPersonalAccessTokensClient</c> and
///         <c>IApplicationsClient</c>.
///     </para>
/// </summary>
public interface ICurrentUserClient
{
    /// <summary>
    ///     Streams the last-activity record of every user on the instance.
    ///     <para>
    ///         Despite the <c>/user</c> prefix this is an <em>administrator</em> report, not a report on the
    ///         token's owner: it requires instance administrator rights and answers
    ///         <see cref="Exceptions.GitLabForbiddenException" /> otherwise. GitLab looks back six months
    ///         when <see cref="UserActivityListOptions.From" /> is not set.
    ///     </para>
    /// </summary>
    IAsyncEnumerable<GitLabUserActivity> ListActivitiesAsync(UserActivityListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the secondary email addresses on the account. The primary address is not among them - it
    ///     is a property of the user and comes back from <c>GET /user</c>.
    /// </summary>
    IAsyncEnumerable<GitLabEmail> ListEmailsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one secondary email address by <em>its own</em> id, which is not the user's id.</summary>
    Task<GitLabEmail> GetEmailAsync(long emailId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a secondary email address. GitLab sends a confirmation mail, so the returned address has a
    ///     null <see cref="GitLabEmail.ConfirmedAt" /> and receives no notifications until it is confirmed.
    /// </summary>
    Task<GitLabEmail> AddEmailAsync(AddCurrentUserEmailRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a secondary email address. The primary address cannot be deleted this way; mail bound for
    ///     a deleted address is redirected to the primary one.
    /// </summary>
    Task DeleteEmailAsync(long emailId, CancellationToken cancellationToken = default);

    /// <summary>Gets the account's preferences - the diff-viewing and CI-identity flags the API exposes.</summary>
    Task<GitLabUserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the account's preferences. Despite the <c>PUT</c>, omitted members are left alone rather
    ///     than cleared.
    /// </summary>
    Task<GitLabUserPreferences> UpdatePreferencesAsync(UpdateUserPreferencesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the account's status. A user who never set one still gets a <c>200</c>, with empty members
    ///     rather than a <see cref="Exceptions.GitLabNotFoundException" />.
    /// </summary>
    Task<GitLabUserStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     <em>Replaces</em> the status (<c>PUT /user/status</c>): every field the request omits is
    ///     nullified, so a request carrying only a message clears the emoji and the availability. To amend
    ///     one field and leave the rest standing, use <see cref="UpdateStatusAsync" />.
    /// </summary>
    Task<GitLabUserStatus> SetStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     <em>Amends</em> the status (<c>PATCH /user/status</c>): fields the request omits keep their
    ///     current value. The counterpart to <see cref="SetStatusAsync" />, which clears them.
    /// </summary>
    Task<GitLabUserStatus> UpdateStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the current Support PIN, if one has been created and has not expired. GitLab Support asks
    ///     for it to verify identity; treat the returned <see cref="GitLabSupportPin.Pin" /> as a secret.
    /// </summary>
    Task<GitLabSupportPin> GetSupportPinAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a Support PIN, valid for seven days. Creating a new one supersedes any existing PIN.</summary>
    Task<GitLabSupportPin> CreateSupportPinAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a runner owned by the authenticated user - the supported replacement for the removed
    ///     registration-token flow.
    ///     <para>
    ///         The returned <see cref="GitLabRunnerRegistration.Token" /> is shown exactly once and cannot
    ///         be read back from any later call; persist it before doing anything else with the result.
    ///     </para>
    /// </summary>
    Task<GitLabRunnerRegistration> CreateRunnerAsync(CreateUserRunnerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads the account's avatar as <c>multipart/form-data</c> and returns where GitLab put it.
    ///     <para>
    ///         The stream on <paramref name="avatar" /> is read but never disposed - the caller keeps
    ///         ownership. <see cref="GitLabFileUpload.FieldName" /> is ignored: this endpoint names its part
    ///         <c>avatar</c> rather than the usual <c>file</c>, and that is applied for you.
    ///     </para>
    /// </summary>
    Task<GitLabAvatar> SetAvatarAsync(GitLabFileUpload avatar, CancellationToken cancellationToken = default);
}