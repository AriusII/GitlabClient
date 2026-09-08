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