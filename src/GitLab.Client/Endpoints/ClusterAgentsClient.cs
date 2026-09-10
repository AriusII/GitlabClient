using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ClusterAgentsClient(IGitLabApiConnection connection) : IClusterAgentsClient
{
    public IAsyncEnumerable<GitLabClusterAgent> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Build(),
            GitLabJsonContext.Default.GitLabClusterAgentArray,
            cancellationToken);
    }

    public Task<GitLabClusterAgent> CreateAsync(ProjectId projectId, CreateClusterAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterAgentRequest,
            GitLabJsonContext.Default.GitLabClusterAgent,
            cancellationToken);
    }

    public Task<GitLabClusterAgent> GetAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Build(),
            GitLabJsonContext.Default.GitLabClusterAgent,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long agentId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabClusterAgentToken> ListTokensAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("tokens").Build(),
            GitLabJsonContext.Default.GitLabClusterAgentTokenArray,
            cancellationToken);
    }

    public Task<GitLabClusterAgentTokenWithSecret> CreateTokenAsync(ProjectId projectId, long agentId,
        CreateClusterAgentTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("tokens").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterAgentTokenRequest,
            GitLabJsonContext.Default.GitLabClusterAgentTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabClusterAgentToken> GetTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("tokens").Segment(tokenId).Build(),
            GitLabJsonContext.Default.GitLabClusterAgentToken,
            cancellationToken);
    }

    public Task RevokeTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("tokens").Segment(tokenId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabClusterAgentUrlConfiguration> ListUrlConfigurationsAsync(ProjectId projectId,
        long agentId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("url_configurations").Build(),
            GitLabJsonContext.Default.GitLabClusterAgentUrlConfigurationArray,
            cancellationToken);
    }

    public Task<GitLabClusterAgentUrlConfiguration> CreateUrlConfigurationAsync(ProjectId projectId, long agentId,
        CreateClusterAgentUrlConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("url_configurations").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterAgentUrlConfigurationRequest,
            GitLabJsonContext.Default.GitLabClusterAgentUrlConfiguration,
            cancellationToken);
    }

    public Task<GitLabClusterAgentUrlConfiguration> GetUrlConfigurationAsync(ProjectId projectId, long agentId,
        long urlConfigurationId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("url_configurations").Segment(urlConfigurationId).Build(),
            GitLabJsonContext.Default.GitLabClusterAgentUrlConfiguration,
            cancellationToken);
    }

    public Task DeleteUrlConfigurationAsync(ProjectId projectId, long agentId, long urlConfigurationId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("cluster_agents").Segment(agentId)
                .Literal("url_configurations").Segment(urlConfigurationId).Build(),
            cancellationToken);
    }
}