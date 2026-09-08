using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Typed per-slug setters for the Integrations resource, part B: Confluence, the Custom Issue
///     Tracker, Datadog, Diffblue Cover, Discord, Drone CI, Emails on Push, IBM EWM, the External Wiki,
///     GitGuardian, GitHub, and the GitLab for Slack app.
///     <para>
///         Each is a thin wrapper over <see cref="SetAsync{TSettings}" /> - see
///         <c>IIntegrationsRepository.cs</c> for why the resource is split into a generic core plus
///         per-slug partial files. The vendored spec also lists every one of these under the deprecated
///         <c>/projects/:id/services/&lt;slug&gt;</c> alias (for example
///         <c>putApiV4ProjectsIdServicesConfluence</c>); that alias is byte-identical to
///         <c>/projects/:id/integrations/&lt;slug&gt;</c> in slug, verb and schema, so these setters
///         target the modern <c>/integrations</c> route rather than doubling the surface with a second
///         wrapper for the abandoned name.
///     </para>
/// </summary>
internal partial interface IIntegrationsRepository
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