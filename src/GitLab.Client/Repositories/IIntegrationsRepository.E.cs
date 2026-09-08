using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part E of the Integrations resource: typed setters for the seven integrations this slice of the
///     spec covers - Squash TM, TeamCity, Telegram, Unify Circuit, Webex Teams, YouTrack and ZenTao -
///     addressed through GitLab's older <c>/projects/:id/services/...</c> path spelling (see
///     <see cref="IntegrationsRepository" /> for why that alias exists alongside <c>/integrations/...</c>).
///     Also carries the two slug-generic operations on that same alias, <see cref="GetServiceAsync" />
///     and <see cref="DisableServiceAsync" />, which this slice of the spec happened to include.
/// </summary>
internal partial interface IIntegrationsRepository
{
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCitySettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);
}