namespace GitLab.Client.Models;

/// <summary>
///     The enabled state of one foundational agent in the
///     <c>foundational_agents_statuses</c> collection accepted by <c>PUT /groups/:id</c>.
/// </summary>
public sealed record GitLabFoundationalAgentStatus
{
    /// <summary>The foundational agent's reference.</summary>
    public required string? Reference { get; init; }

    /// <summary>Whether GitLab enables the foundational agent.</summary>
    public required bool? Enabled { get; init; }
}