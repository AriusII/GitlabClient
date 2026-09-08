using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part E of the Integrations resource: typed setters for Squash TM, TeamCity, Telegram, Unify
///     Circuit, Webex Teams, YouTrack and ZenTao. These seven used to be reachable only through GitLab's
///     older <c>/projects/:id/services/:slug</c> alias - the exact alias <see cref="IIntegrationsRepository" />'s
///     own remarks say is deliberately not wrapped, since it would double the surface for nothing - and
///     now address the modern <c>/projects/:id/integrations/:slug</c> route like every other setter in
///     this resource (see <c>IIntegrationsRepository.B.cs</c> / <c>IIntegrationsRepository.C.cs</c>).
///     Also carries the slug-generic <see cref="GetServiceAsync" /> / <see cref="DisableServiceAsync" />
///     pair, which still exercises the <c>/services</c> alias directly and is kept only because it is
///     not (yet) the only way to reach a slug - <see cref="IIntegrationsRepository.GetAsync" /> and
///     <see cref="IIntegrationsRepository.DisableAsync" /> already cover every slug through the modern
///     route.
/// </summary>
internal partial interface IIntegrationsRepository
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