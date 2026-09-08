using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Typed per-slug setters for the Integrations client, part B: Confluence, the Custom Issue
///     Tracker, Datadog, Diffblue Cover, Discord, Drone CI, Emails on Push, IBM EWM, the External Wiki,
///     GitGuardian, GitHub, and the GitLab for Slack app.
///     <para>
///         Each is a thin, strongly typed wrapper over
///         <see cref="IIntegrationsClient.SetAsync{TSettings}" />, saving a caller from having to know
///         the slug string or hand-build a settings dictionary for one of these particular integrations.
///         The generic <c>SetAsync</c>/<c>SetAsync&lt;TSettings&gt;</c> overloads remain the way to reach
///         any of the ~50 other slugs this library has no dedicated type for.
///     </para>
///     <para>
///         The vendored spec lists these under the deprecated <c>/projects/:id/services/&lt;slug&gt;</c>
///         alias (for example <c>PUT /projects/:id/services/confluence</c>). That alias answers with the
///         same slug, verb and schema as <c>/projects/:id/integrations/&lt;slug&gt;</c>, so these setters
///         call through the modern <c>/integrations</c> route instead of wrapping the abandoned name a
///         second time - see the remarks on <see cref="IIntegrationsClient" /> itself.
///     </para>
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Creates or updates the Confluence Workspace integration on a project.</summary>
    Task<GitLabIntegration> SetConfluenceAsync(ProjectId projectId, ConfluenceIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Custom Issue Tracker integration on a project.</summary>
    Task<GitLabIntegration> SetCustomIssueTrackerAsync(ProjectId projectId,
        CustomIssueTrackerIntegrationRequest settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Datadog integration on a project.</summary>
    Task<GitLabIntegration> SetDatadogAsync(ProjectId projectId, DatadogIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Diffblue Cover integration on a project.</summary>
    Task<GitLabIntegration> SetDiffblueCoverAsync(ProjectId projectId, DiffblueCoverIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Discord Notifications integration on a project.</summary>
    Task<GitLabIntegration> SetDiscordAsync(ProjectId projectId, DiscordIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Drone CI integration on a project.</summary>
    Task<GitLabIntegration> SetDroneCiAsync(ProjectId projectId, DroneCiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Emails on Push integration on a project.</summary>
    Task<GitLabIntegration> SetEmailsOnPushAsync(ProjectId projectId, EmailsOnPushIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the IBM Engineering Workflow Management integration on a project.</summary>
    Task<GitLabIntegration> SetEwmAsync(ProjectId projectId, EwmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the External Wiki integration on a project.</summary>
    Task<GitLabIntegration> SetExternalWikiAsync(ProjectId projectId, ExternalWikiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the GitGuardian integration on a project.</summary>
    Task<GitLabIntegration> SetGitGuardianAsync(ProjectId projectId, GitGuardianIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the GitHub integration on a project.</summary>
    Task<GitLabIntegration> SetGitHubAsync(ProjectId projectId, GitHubIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the GitLab for Slack app integration on a project.</summary>
    Task<GitLabIntegration> SetGitLabSlackApplicationAsync(ProjectId projectId,
        GitLabSlackApplicationIntegrationRequest settings, CancellationToken cancellationToken = default);
}