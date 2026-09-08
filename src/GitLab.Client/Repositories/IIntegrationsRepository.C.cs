using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part C of the per-slug typed setters promised by <see cref="IIntegrationsRepository" />'s primary
///     declaration: Google Cloud Platform Artifact Registry, Google Cloud Platform Workload Identity
///     Federation, Google Play, Hangouts Chat, Harbor, Irker, Jenkins, Jira, the Jira Cloud App, Linear,
///     Matrix and Mattermost. Each is a thin wrapper over the generic
///     <c>SetAsync{TSettings}(ProjectId, string, TSettings, JsonTypeInfo{TSettings}, CancellationToken)</c>
///     declared on the primary partial, addressing its slug via <see cref="GitLabIntegrationSlug" /> and
///     its own <see cref="Infrastructure.Serialization.GitLabJsonContext" /> entry - the same shape the
///     Slack partial (<c>IIntegrationsRepository.Slack.cs</c>) demonstrates.
///     <para>
///         GitLab also exposes every one of these under the former <c>/projects/:id/services/...</c>
///         path (byte-identical slugs, verbs and schemas). That alias is deliberately not wrapped here,
///         for the same reason documented on <see cref="IIntegrationsRepository" /> and
///         <c>Abstractions.IIntegrationsClient</c>: it is this same API under an abandoned name, and the
///         modern <c>/integrations/:slug</c> route this partial calls through already reaches it.
///     </para>
/// </summary>
internal partial interface IIntegrationsRepository
{
    Task<GitLabIntegration> SetGoogleCloudPlatformArtifactRegistryAsync(ProjectId projectId,
        GoogleCloudPlatformArtifactRegistryIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId, HangoutsChatIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId, JiraCloudAppIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationRequest settings,
        CancellationToken cancellationToken = default);
}