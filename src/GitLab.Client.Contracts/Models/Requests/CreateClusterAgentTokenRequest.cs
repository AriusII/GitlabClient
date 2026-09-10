namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/cluster_agents/:agent_id/tokens</c>. An agent can hold only
///     two active tokens at a time.
/// </summary>
public sealed record CreateClusterAgentTokenRequest
{
    /// <summary>The name for the token.</summary>
    public required string Name { get; init; }

    /// <summary>The description for the token.</summary>
    public string? Description { get; init; }
}