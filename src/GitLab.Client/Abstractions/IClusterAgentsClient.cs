using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Cluster agents" API area (<c>/projects/:id/cluster_agents</c>): registering the
///     <see href="https://docs.gitlab.com/user/clusters/agent/">GitLab agent for Kubernetes</see> against
///     a project, minting the tokens it authenticates with, and - for a receptive agent - the URL
///     configuration GitLab uses to connect to it.
///     <para>
///         Not to be confused with the deprecated, certificate-based Kubernetes cluster integration
///         (GitLab's "Clusters" API): this is the current, agent-based integration and carries no
///         deprecation of its own.
///     </para>
/// </summary>
public interface IClusterAgentsClient
{
    /// <summary>
    ///     Lists every agent registered for the project, streaming every page. Requires the Developer,
    ///     Maintainer, or Owner role.
    /// </summary>
    IAsyncEnumerable<GitLabClusterAgent> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Registers an agent for the project. Requires the Maintainer or Owner role.</summary>
    Task<GitLabClusterAgent> CreateAsync(ProjectId projectId, CreateClusterAgentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves details on one agent. Requires the Developer, Maintainer, or Owner role.</summary>
    Task<GitLabClusterAgent> GetAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an agent's registration. Requires the Maintainer or Owner role.</summary>
    Task DeleteAsync(ProjectId projectId, long agentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists an agent's active tokens, streaming every page. Metadata only - see
    ///     <see cref="CreateTokenAsync" /> for the one call that returns a usable secret. Requires the
    ///     Developer, Maintainer, or Owner role.
    /// </summary>
    IAsyncEnumerable<GitLabClusterAgentToken> ListTokensAsync(ProjectId projectId, long agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a token for an agent. An agent can hold only two active tokens at a time. Requires the
    ///     Maintainer or Owner role.
    ///     <para>
    ///         This is the only call that returns a usable secret, and it returns it exactly once - see
    ///         <see cref="GitLabClusterAgentTokenWithSecret.Token" />. Persist it immediately, and keep it
    ///         out of logs.
    ///     </para>
    /// </summary>
    Task<GitLabClusterAgentTokenWithSecret> CreateTokenAsync(ProjectId projectId, long agentId,
        CreateClusterAgentTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one token's metadata. The secret is never returned here. GitLab answers with a
    ///     <see cref="Exceptions.GitLabNotFoundException" /> if the token has been revoked. Requires the
    ///     Developer, Maintainer, or Owner role.
    /// </summary>
    Task<GitLabClusterAgentToken> GetTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes a token immediately. An agent still using it stops authenticating. Requires the
    ///     Maintainer or Owner role.
    /// </summary>
    Task RevokeTokenAsync(ProjectId projectId, long agentId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists an agent's URL configurations, streaming every page. An agent can hold only one at a
    ///     time, so this is effectively zero-or-one item. Introduced in GitLab 17.4. Requires the
    ///     Developer, Maintainer, or Owner role.
    /// </summary>
    IAsyncEnumerable<GitLabClusterAgentUrlConfiguration> ListUrlConfigurationsAsync(ProjectId projectId,
        long agentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a URL configuration for a receptive agent - the address and TLS material GitLab uses to
    ///     connect to it. An agent can hold only one at a time. Introduced in GitLab 17.4. Requires the
    ///     Maintainer or Owner role.
    /// </summary>
    Task<GitLabClusterAgentUrlConfiguration> CreateUrlConfigurationAsync(ProjectId projectId, long agentId,
        CreateClusterAgentUrlConfigurationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one URL configuration. Introduced in GitLab 17.4. Requires the Developer, Maintainer,
    ///     or Owner role.
    /// </summary>
    Task<GitLabClusterAgentUrlConfiguration> GetUrlConfigurationAsync(ProjectId projectId, long agentId,
        long urlConfigurationId, CancellationToken cancellationToken = default);

    /// <summary>Deletes a URL configuration. Introduced in GitLab 17.4. Requires the Maintainer or Owner role.</summary>
    Task DeleteUrlConfigurationAsync(ProjectId projectId, long agentId, long urlConfigurationId,
        CancellationToken cancellationToken = default);
}