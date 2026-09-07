using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the authenticated user's own account: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICurrentUserService), typeof(ICurrentUserClient))]
internal interface ICurrentUserRepository
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