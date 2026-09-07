using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Notification settings resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(INotificationSettingsService), typeof(INotificationSettingsClient))]
internal interface INotificationSettingsRepository
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