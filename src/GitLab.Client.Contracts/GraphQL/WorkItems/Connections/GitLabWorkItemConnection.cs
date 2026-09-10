using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>A cursor-paginated GraphQL connection of work items.</summary>
/// <remarks>
///     An empty <see cref="Nodes" /> collection means the selected connection has no work items. A null collection
///     means <c>nodes</c> was not selected or the containing resource was unavailable, and must not be treated as an
///     empty page. A null node represents a partial GraphQL response; consult the response envelope's
///     <c>errors</c> collection before treating it as an absent work item.
/// </remarks>
public sealed record GitLabWorkItemConnection
{
    /// <summary>The selected work-item nodes.</summary>
    [JsonPropertyName("nodes")]
    public IReadOnlyList<GitLabWorkItem?>? Nodes { get; init; }

    /// <summary>The selected cursor metadata.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabWorkItemPageInfo? PageInfo { get; init; }

    /// <summary>The bounded GitLab connection count, when the query selected it.</summary>
    [JsonPropertyName("count")]
    public int? Count { get; init; }
}