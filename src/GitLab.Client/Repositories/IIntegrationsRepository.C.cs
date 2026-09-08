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
        GoogleCloudPlatformArtifactRegistryIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId, HangoutsChatIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId, JiraCloudAppIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationSettings settings,
        CancellationToken cancellationToken = default);
}