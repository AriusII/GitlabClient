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
        GoogleCloudPlatformArtifactRegistryIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GoogleCloudPlatformArtifactRegistry, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformArtifactRegistryIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GoogleCloudPlatformWorkloadIdentityFederation, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.GooglePlay, settings,
            GitLabJsonContext.Default.GooglePlayIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId,
        HangoutsChatIntegrationRequest settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.HangoutsChat, settings,
            GitLabJsonContext.Default.HangoutsChatIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Harbor, settings,
            GitLabJsonContext.Default.HarborIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Irker, settings,
            GitLabJsonContext.Default.IrkerIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Jenkins, settings,
            GitLabJsonContext.Default.JenkinsIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Jira, settings,
            GitLabJsonContext.Default.JiraIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId,
        JiraCloudAppIntegrationRequest settings, CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.JiraCloudApp, settings,
            GitLabJsonContext.Default.JiraCloudAppIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Linear, settings,
            GitLabJsonContext.Default.LinearIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Matrix, settings,
            GitLabJsonContext.Default.MatrixIntegrationRequest, cancellationToken);
    }

    public Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationRequest settings,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(projectId, GitLabIntegrationSlug.Mattermost, settings,
            GitLabJsonContext.Default.MattermostIntegrationRequest, cancellationToken);
    }
}