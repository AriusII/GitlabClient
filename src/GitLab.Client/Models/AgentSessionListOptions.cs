using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /projects/:id/ai_agent/sessions</c>.</summary>
[GitLabQuery]
public readonly record struct AgentSessionListOptions
{
    /// <summary>Return only sessions run by this agent.</summary>
    public GitLabAgentType? AgentType { get; init; }

    /// <summary>Return only sessions in this lifecycle state.</summary>
    public GitLabAgentSessionStatus? Status { get; init; }

    /// <summary>Return only sessions created strictly after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only sessions created strictly before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}