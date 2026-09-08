using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed partial class IntegrationsRepository
{
    public Task<GitLabIntegration> SetConfluenceAsync(ProjectId projectId, ConfluenceIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Confluence, settings,
            GitLabJsonContext.Default.ConfluenceIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetCustomIssueTrackerAsync(ProjectId projectId,
        CustomIssueTrackerIntegrationRequest settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.CustomIssueTracker, settings,
            GitLabJsonContext.Default.CustomIssueTrackerIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetDatadogAsync(ProjectId projectId, DatadogIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Datadog, settings,
            GitLabJsonContext.Default.DatadogIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetDiffblueCoverAsync(ProjectId projectId,
        DiffblueCoverIntegrationRequest settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.DiffblueCover, settings,
            GitLabJsonContext.Default.DiffblueCoverIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetDiscordAsync(ProjectId projectId, DiscordIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Discord, settings,
            GitLabJsonContext.Default.DiscordIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetDroneCiAsync(ProjectId projectId, DroneCiIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.DroneCi, settings,
            GitLabJsonContext.Default.DroneCiIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetEmailsOnPushAsync(ProjectId projectId, EmailsOnPushIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.EmailsOnPush, settings,
            GitLabJsonContext.Default.EmailsOnPushIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetEwmAsync(ProjectId projectId, EwmIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Ewm, settings,
            GitLabJsonContext.Default.EwmIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetExternalWikiAsync(ProjectId projectId, ExternalWikiIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.ExternalWiki, settings,
            GitLabJsonContext.Default.ExternalWikiIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetGitGuardianAsync(ProjectId projectId, GitGuardianIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GitGuardian, settings,
            GitLabJsonContext.Default.GitGuardianIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetGitHubAsync(ProjectId projectId, GitHubIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GitHub, settings,
            GitLabJsonContext.Default.GitHubIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetGitLabSlackApplicationAsync(ProjectId projectId,
        GitLabSlackApplicationIntegrationRequest settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GitLabSlackApplication, settings,
            GitLabJsonContext.Default.GitLabSlackApplicationIntegrationRequest, cancellationToken);
    }
}