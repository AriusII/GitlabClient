using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Part E of <see cref="IIntegrationsClient" />: typed setters for Squash TM, TeamCity, Telegram,
///     Unify Circuit, Webex Teams, YouTrack and ZenTao. These seven used to be reachable only through
///     GitLab's older <c>/projects/:id/services/:slug</c> alias - the exact alias this client's own
///     remarks say is deliberately not wrapped, since it would double the surface for nothing - and now
///     address the modern <c>/projects/:id/integrations/:slug</c> route like every other setter on this
///     client. Also carries the slug-generic <see cref="GetServiceAsync" /> / <see cref="DisableServiceAsync" />
///     pair, kept only for the <c>/services</c> alias itself; <see cref="IIntegrationsClient.GetAsync" />
///     and <see cref="IIntegrationsClient.DisableAsync" /> already reach every slug through the modern
///     route, so both are <see cref="ObsoleteAttribute" />.
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Creates or updates the Squash TM integration.</summary>
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the JetBrains TeamCity integration.</summary>
    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCityIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Telegram integration.</summary>
    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Unify Circuit integration.</summary>
    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Webex Teams integration.</summary>
    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the YouTrack integration.</summary>
    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the ZenTao integration.</summary>
    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one project integration's settings by slug through the <c>/services</c> alias route.
    ///     Byte-identical to <see cref="IIntegrationsClient.GetAsync" />, which calls the same integration
    ///     through its current <c>/integrations</c> path. Throws <see cref="Exceptions.GitLabNotFoundException" />
    ///     when the integration has never been configured on the project.
    /// </summary>
    [Obsolete("Use the modern /integrations route instead (GetAsync).")]
    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Disables a project integration and discards its settings through the <c>/services</c> alias
    ///     route. Byte-identical to <see cref="IIntegrationsClient.DisableAsync" />.
    /// </summary>
    [Obsolete("Use the modern /integrations route instead (DisableAsync).")]
    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);
}