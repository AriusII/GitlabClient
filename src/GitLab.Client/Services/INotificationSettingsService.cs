using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Notification settings, sitting between the public
///     <c>INotificationSettingsClient</c> controller and <c>INotificationSettingsRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is generated);
///     this is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface INotificationSettingsService
{
    Task<GitLabNotificationSettings> GetGlobalAsync(CancellationToken cancellationToken = default);

    Task<GitLabNotificationSettings> UpdateGlobalAsync(UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNotificationSettings> GetForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabNotificationSettings> UpdateForGroupAsync(GroupId groupId, UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNotificationSettings> GetForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabNotificationSettings> UpdateForProjectAsync(ProjectId projectId,
        UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default);
}