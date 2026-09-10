using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class PlatformIntegrationsClient(IGitLabApiConnection connection)
    : IPlatformIntegrationsClient
{
    public Task<GitLabJob> GetAllowedAgentsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("job").Literal("allowed_agents").Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<JsonElement> GetIamUserInfoAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("iam").Literal("userinfo").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetGoogleCloudIntegrationSetupScriptAsync(ProjectId projectId,
        GoogleCloudIntegrationSetupScriptOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("google_cloud")
                .Literal("setup")
                .Literal("integrations.sh")
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetGoogleCloudRunnerDeploymentSetupScriptAsync(ProjectId projectId,
        string googleCloudProjectId, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("google_cloud")
                .Literal("setup")
                .Literal("runner_deployment_project.sh")
                .Query("google_cloud_project_id", googleCloudProjectId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabTokenExchangeResult> ExchangeTokenAsync(TokenExchangeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("token_exchange").Build(),
            request,
            GitLabJsonContext.Default.TokenExchangeRequest,
            GitLabJsonContext.Default.GitLabTokenExchangeResult,
            cancellationToken);
    }
}