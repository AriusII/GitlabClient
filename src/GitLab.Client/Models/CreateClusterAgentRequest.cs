namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/cluster_agents</c>.</summary>
public sealed record CreateClusterAgentRequest
{
    /// <summary>The name of the agent.</summary>
    public required string Name { get; init; }
}