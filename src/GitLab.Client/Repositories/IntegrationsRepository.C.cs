using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Implements the part-C per-slug typed setters declared on <c>IIntegrationsRepository.C.cs</c>.
///     Every method here is a one-line forward to the generic <c>SetAsync{TSettings}</c> already defined
///     on the primary partial (<c>IntegrationsRepository.cs</c>), which builds the
///     <c>/projects/:id/integrations/:slug</c> route and issues the <c>PUT</c>.
/// </summary>
internal sealed partial class IntegrationsRepository
{
    public Task<GitLabIntegration> SetGoogleCloudPlatformArtifactRegistryAsync(ProjectId projectId,
        GoogleCloudPlatformArtifactRegistryIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GoogleCloudPlatformArtifactRegistry, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformArtifactRegistryIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GoogleCloudPlatformWorkloadIdentityFederation, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformWorkloadIdentityFederationIntegrationSettings,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GooglePlay, settings,
            GitLabJsonContext.Default.GooglePlayIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId,
        HangoutsChatIntegrationSettings settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.HangoutsChat, settings,
            GitLabJsonContext.Default.HangoutsChatIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Harbor, settings,
            GitLabJsonContext.Default.HarborIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Irker, settings,
            GitLabJsonContext.Default.IrkerIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Jenkins, settings,
            GitLabJsonContext.Default.JenkinsIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Jira, settings,
            GitLabJsonContext.Default.JiraIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId,
        JiraCloudAppIntegrationSettings settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.JiraCloudApp, settings,
            GitLabJsonContext.Default.JiraCloudAppIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Linear, settings,
            GitLabJsonContext.Default.LinearIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Matrix, settings,
            GitLabJsonContext.Default.MatrixIntegrationSettings, cancellationToken);
    }

    public Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationSettings settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Mattermost, settings,
            GitLabJsonContext.Default.MattermostIntegrationSettings, cancellationToken);
    }
}