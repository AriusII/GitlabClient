using GitLab.Client.Abstractions;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the authenticated user's own account, sitting between the public
///     <c>ICurrentUserClient</c> controller and <c>ICurrentUserRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface ICurrentUserService
{
    IAsyncEnumerable<GitLabUserActivity> ListActivitiesAsync(UserActivityListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabEmail> ListEmailsAsync(CancellationToken cancellationToken = default);

    Task<GitLabEmail> GetEmailAsync(long emailId, CancellationToken cancellationToken = default);

    Task<GitLabEmail> AddEmailAsync(AddCurrentUserEmailRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteEmailAsync(long emailId, CancellationToken cancellationToken = default);

    Task<GitLabUserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default);

    Task<GitLabUserPreferences> UpdatePreferencesAsync(UpdateUserPreferencesRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabUserStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<GitLabUserStatus> SetStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabUserStatus> UpdateStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSupportPin> GetSupportPinAsync(CancellationToken cancellationToken = default);

    Task<GitLabSupportPin> CreateSupportPinAsync(CancellationToken cancellationToken = default);

    Task<GitLabRunnerRegistration> CreateRunnerAsync(CreateUserRunnerRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabAvatar> SetAvatarAsync(GitLabFileUpload avatar, CancellationToken cancellationToken = default);
}