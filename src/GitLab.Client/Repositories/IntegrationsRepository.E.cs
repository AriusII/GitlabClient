using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part E of the Integrations repository - see <see cref="IIntegrationsRepository" />'s part-E
///     declaration for what this file covers.
/// </summary>
internal sealed partial class IntegrationsRepository
{
    /// <summary>
    ///     The fixed path word GitLab's older, still-served alias for this whole resource is built on -
    ///     see the remarks on the primary partial declaration of this class for why both spellings exist.
    /// </summary>
    private const string Services = "services";

    public Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.SquashTm).Build(),
            settings,
            GitLabJsonContext.Default.SquashTmSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCitySettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.TeamCity).Build(),
            settings,
            GitLabJsonContext.Default.TeamCitySettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.Telegram).Build(),
            settings,
            GitLabJsonContext.Default.TelegramSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.UnifyCircuit).Build(),
            settings,
            GitLabJsonContext.Default.UnifyCircuitSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.WebexTeams).Build(),
            settings,
            GitLabJsonContext.Default.WebexTeamsSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.YouTrack).Build(),
            settings,
            GitLabJsonContext.Default.YouTrackSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoSettingsRequest settings,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services)
                .Literal(GitLabIntegrationSlug.Zentao).Build(),
            settings,
            GitLabJsonContext.Default.ZentaoSettingsRequest,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectServiceRoute(projectId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

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