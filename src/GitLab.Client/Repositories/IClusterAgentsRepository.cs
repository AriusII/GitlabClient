using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ClusterAgents resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IClusterAgentsService), typeof(IClusterAgentsClient))]
internal interface IClusterAgentsRepository
{
    IAsyncEnumerable<GitLabClusterAgent> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabClusterAgent> CreateAsync(ProjectId projectId, CreateClusterAgentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabClusterAgent> GetAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long agentId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabClusterAgentToken> ListTokensAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default);

    Task<GitLabClusterAgentTokenWithSecret> CreateTokenAsync(ProjectId projectId, long agentId,
        CreateClusterAgentTokenRequest request, CancellationToken cancellationToken = default);

    Task<GitLabClusterAgentToken> GetTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabClusterAgentUrlConfiguration> ListUrlConfigurationsAsync(ProjectId projectId,
        long agentId, CancellationToken cancellationToken = default);

    Task<GitLabClusterAgentUrlConfiguration> CreateUrlConfigurationAsync(ProjectId projectId, long agentId,
        CreateClusterAgentUrlConfigurationRequest request, CancellationToken cancellationToken = default);

    Task<GitLabClusterAgentUrlConfiguration> GetUrlConfigurationAsync(ProjectId projectId, long agentId,
        long urlConfigurationId, CancellationToken cancellationToken = default);

    Task DeleteUrlConfigurationAsync(ProjectId projectId, long agentId, long urlConfigurationId,
        CancellationToken cancellationToken = default);
}