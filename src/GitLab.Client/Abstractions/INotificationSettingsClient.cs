using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Notification settings" API area (<c>/notification_settings</c>,
///     <c>/groups/:id/notification_settings</c>, <c>/projects/:id/notification_settings</c>) - the
///     authenticated user's own notification preferences, globally or scoped to one group or project, which
///     overrides the global setting.
/// </summary>
public interface INotificationSettingsClient
{
    /// <summary>Gets the authenticated user's global notification level and notification email.</summary>
    Task<GitLabNotificationSettings> GetGlobalAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the authenticated user's global notification settings.</summary>
    Task<GitLabNotificationSettings> UpdateGlobalAsync(UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the authenticated user's notification level for one group.</summary>
    Task<GitLabNotificationSettings> GetForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Updates the authenticated user's notification settings for one group.</summary>
    Task<GitLabNotificationSettings> UpdateForGroupAsync(GroupId groupId, UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the authenticated user's notification level for one project.</summary>
    Task<GitLabNotificationSettings> GetForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the authenticated user's notification settings for one project.</summary>
    Task<GitLabNotificationSettings> UpdateForProjectAsync(ProjectId projectId,
        UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default);
}