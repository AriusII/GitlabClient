using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Part C of <see cref="IIntegrationsClient" />'s typed per-slug setters: Google Cloud Platform
///     Artifact Registry, Google Cloud Platform Workload Identity Federation, Google Play, Hangouts
///     Chat (Google Chat), Harbor, Irker, Jenkins, Jira, the Jira Cloud App, Linear, Matrix and
///     Mattermost. Each wraps the generic <c>SetAsync{TSettings}</c> declared on the primary partial
///     with the integration's own typed settings record, so a caller never has to spell the slug or
///     wire up a <c>JsonTypeInfo</c> by hand for these twelve.
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Creates or updates the Google Cloud Platform Artifact Registry integration on a project.</summary>
    Task<GitLabIntegration> SetGoogleCloudPlatformArtifactRegistryAsync(ProjectId projectId,
        GoogleCloudPlatformArtifactRegistryIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or updates the Google Cloud Platform Workload Identity Federation integration on a
    ///     project.
    /// </summary>
    Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Google Play integration on a project.</summary>
    Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Hangouts Chat (Google Chat) integration on a project.</summary>
    Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId, HangoutsChatIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Harbor integration on a project.</summary>
    Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Irker (IRC gateway) integration on a project.</summary>
    Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Jenkins integration on a project.</summary>
    Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Jira integration on a project.</summary>
    Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the GitLab for Jira Cloud app integration on a project.</summary>
    Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId, JiraCloudAppIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Linear integration on a project.</summary>
    Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Matrix integration on a project.</summary>
    Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the Mattermost notifications integration on a project.</summary>
    Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationSettings settings,
        CancellationToken cancellationToken = default);
}