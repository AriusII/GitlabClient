using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class NotificationSettingsClient(IGitLabApiConnection connection)
    : INotificationSettingsClient
{
    public Task<GitLabNotificationSettings> GetGlobalAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("notification_settings").Build(),
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }

    public Task<GitLabNotificationSettings> UpdateGlobalAsync(UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("notification_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateNotificationSettingsRequest,
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }

    public Task<GitLabNotificationSettings> GetForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("notification_settings").Build(),
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }

    public Task<GitLabNotificationSettings> UpdateForGroupAsync(GroupId groupId,
        UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("notification_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateNotificationSettingsRequest,
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }

    public Task<GitLabNotificationSettings> GetForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("notification_settings").Build(),
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }

    public Task<GitLabNotificationSettings> UpdateForProjectAsync(ProjectId projectId,
        UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("notification_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateNotificationSettingsRequest,
            GitLabJsonContext.Default.GitLabNotificationSettings,
            cancellationToken);
    }
}