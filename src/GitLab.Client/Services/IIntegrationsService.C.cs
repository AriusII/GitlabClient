using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Part C of the per-slug typed setters - mirrors <c>IIntegrationsRepository.C.cs</c> member for
///     member, as required for the generated <c>IntegrationsService</c> forwarder to satisfy both
///     interfaces.
/// </summary>
internal partial interface IIntegrationsService
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