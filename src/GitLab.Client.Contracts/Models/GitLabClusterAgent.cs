namespace GitLab.Client.Models;

/// <summary>
///     An agent for the
///     <see href="https://docs.gitlab.com/user/clusters/agent/">
///         GitLab agent for
///         Kubernetes
///     </see>
///     , as returned by the Cluster Agents API
///     (<c>/projects/:id/cluster_agents[/:agent_id]</c>) - the wire's <c>APIEntitiesClustersAgent</c>.
///     <para>
///         An agent is not a Kubernetes cluster itself; it is the registration record for the in-cluster
///         component that connects outward to GitLab. <see cref="ConfigProject" /> is the project holding
///         the agent's <c>.gitlab/agents/&lt;name&gt;/config.yaml</c>, which need not be the same project
///         this agent was listed under once the agent is shared across the group.
///     </para>
/// </summary>
public sealed record GitLabClusterAgent
{
    public required long Id { get; init; }

    /// <summary>The agent's name, unique within its configuration project.</summary>
    public string? Name { get; init; }

    /// <summary>The project holding this agent's configuration directory.</summary>
    public GitLabProjectIdentity? ConfigProject { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The id of the user who registered this agent.</summary>
    public long? CreatedByUserId { get; init; }

    /// <summary>
    ///     Whether this is a receptive agent - one GitLab connects <em>to</em>, at a URL configured via
    ///     <see cref="GitLabClusterAgentUrlConfiguration" />, rather than one that dials out to GitLab.
    /// </summary>
    public bool? IsReceptive { get; init; }
}