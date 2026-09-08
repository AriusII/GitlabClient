using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Typed per-slug setters for the Integrations service contract, part B - mirrors
///     <c>IIntegrationsRepository.B.cs</c> member for member.
/// </summary>
internal partial interface IIntegrationsService
{
    Task<GitLabIntegration> SetConfluenceAsync(ProjectId projectId, ConfluenceIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetCustomIssueTrackerAsync(ProjectId projectId,
        CustomIssueTrackerIntegrationRequest settings, CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetDatadogAsync(ProjectId projectId, DatadogIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetDiffblueCoverAsync(ProjectId projectId, DiffblueCoverIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetDiscordAsync(ProjectId projectId, DiscordIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetDroneCiAsync(ProjectId projectId, DroneCiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetEmailsOnPushAsync(ProjectId projectId, EmailsOnPushIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetEwmAsync(ProjectId projectId, EwmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetExternalWikiAsync(ProjectId projectId, ExternalWikiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGitGuardianAsync(ProjectId projectId, GitGuardianIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGitHubAsync(ProjectId projectId, GitHubIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGitLabSlackApplicationAsync(ProjectId projectId,
        GitLabSlackApplicationIntegrationRequest settings, CancellationToken cancellationToken = default);
}