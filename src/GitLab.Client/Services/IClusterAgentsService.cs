using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ClusterAgents, sitting between the public
///     <c>IClusterAgentsClient</c> controller and <c>IClusterAgentsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IClusterAgentsService
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