using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Part E of the Integrations service seam - mirrors <c>IIntegrationsRepository</c>'s part-E members
///     1:1 (see that interface for the rationale); its implementation is generated.
/// </summary>
internal partial interface IIntegrationsService
{
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCityIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    [Obsolete("Use the modern /integrations route instead (GetAsync).")]
    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    [Obsolete("Use the modern /integrations route instead (DisableAsync).")]
    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);
}