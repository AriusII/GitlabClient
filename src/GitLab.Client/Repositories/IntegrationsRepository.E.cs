using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part E of the Integrations repository: typed setters for Squash TM, TeamCity, Telegram, Unify
///     Circuit, Webex Teams, YouTrack and ZenTao. These seven used to be wrapped only through GitLab's
///     older <c>/projects/:id/services/:slug</c> alias - the exact alias the primary partial's own
///     remarks say is deliberately not wrapped, since it would double the surface for nothing. They now
///     go through the same generic
///     <c>
///         SetAsync{TSettings}(ProjectId, string, TSettings,
///         JsonTypeInfo{TSettings}, CancellationToken)
///     </c>
///     the B and C partials use, reaching the modern
///     <c>/projects/:id/integrations/:slug</c> route (see <c>IIntegrationsRepository.B.cs</c> /
///     <c>IIntegrationsRepository.C.cs</c> for the pattern this follows). Also carries the slug-generic
///     <see cref="GetServiceAsync" /> and <see cref="DisableServiceAsync" /> pair, which still exercise
///     the <c>/services</c> alias directly and are the only reason the <see cref="Services" /> path word
///     survives in this class.
/// </summary>
internal sealed partial class IntegrationsRepository
{
    /// <summary>
    ///     The fixed path word GitLab's older, still-served alias for this whole resource is built on -
    ///     kept only for <see cref="GetServiceAsync" />/<see cref="DisableServiceAsync" /> now that the
    ///     seven per-slug setters below address the modern <c>/integrations</c> route instead.
    /// </summary>
    private const string Services = "services";

    public Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.SquashTm, settings,
            GitLabJsonContext.Default.SquashTmIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCityIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.TeamCity, settings,
            GitLabJsonContext.Default.TeamCityIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Telegram, settings,
            GitLabJsonContext.Default.TelegramIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.UnifyCircuit, settings,
            GitLabJsonContext.Default.UnifyCircuitIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.WebexTeams, settings,
            GitLabJsonContext.Default.WebexTeamsIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.YouTrack, settings,
            GitLabJsonContext.Default.YouTrackIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Zentao, settings,
            GitLabJsonContext.Default.ZentaoIntegrationRequest, cancellationToken);
    }

    [Obsolete("Use the modern /integrations route instead (GetAsync).")]
    public Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectServiceRoute(projectId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    [Obsolete("Use the modern /integrations route instead (DisableAsync).")]
    public Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(ProjectServiceRoute(projectId, slug), cancellationToken);
    }

    /// <summary>
    ///     The slug is caller-supplied text here (unlike the seven fixed-slug setters above, which know
    ///     their slug at compile time and use <c>Literal</c>), so it goes through <c>Escaped</c> - the
    ///     same reasoning as <c>IntegrationsRepository.ProjectIntegrationRoute</c> on the primary partial
    ///     declaration of this class, just against the <c>/services</c> alias instead of <c>/integrations</c>.
    /// </summary>
    private static Uri ProjectServiceRoute(ProjectId projectId, string slug)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services).Escaped(slug).Build();
    }
}