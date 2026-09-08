using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Part E of <see cref="IIntegrationsClient" />: typed setters for Squash TM, TeamCity, Telegram,
///     Unify Circuit, Webex Teams, YouTrack and ZenTao, plus the slug-generic get/disable pair - all
///     addressed through GitLab's older <c>/projects/:id/services/...</c> path spelling rather than the
///     current <c>/projects/:id/integrations/...</c> one (see the remarks on <see cref="IIntegrationsClient" />
///     for why both exist). Functionally these reach the very same settings as the base interface's
///     <c>/integrations</c> equivalents - <c>GetAsync</c>, <c>SetAsync&lt;TSettings&gt;</c> and
///     <c>DisableAsync</c> - the spec simply still lists the <c>/services</c> route as a separate
///     operation per integration, so it is wrapped for completeness.
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Creates or updates the Squash TM integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the JetBrains TeamCity integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCitySettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Telegram integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Unify Circuit integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Webex Teams integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the YouTrack integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the ZenTao integration via the <c>/services</c> alias.</summary>
    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoSettingsRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one project integration's settings by slug through the <c>/services</c> alias route.
    ///     Byte-identical to <see cref="IIntegrationsClient.GetAsync" />, which calls the same integration
    ///     through its current <c>/integrations</c> path; prefer that overload unless a caller specifically
    ///     needs to exercise the legacy route. Throws <see cref="Exceptions.GitLabNotFoundException" />
    ///     when the integration has never been configured on the project.
    /// </summary>
    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Disables a project integration and discards its settings through the <c>/services</c> alias
    ///     route. Byte-identical to <see cref="IIntegrationsClient.DisableAsync" />.
    /// </summary>
    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);
}