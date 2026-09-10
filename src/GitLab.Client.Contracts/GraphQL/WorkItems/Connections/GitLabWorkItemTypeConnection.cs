using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>A cursor-paginated GraphQL connection of namespace-specific work-item types.</summary>
/// <remarks>
///     An empty <see cref="Nodes" /> collection means the selected namespace has no matching types. A null collection
///     means the connection fields were not selected or could not be resolved. A null node represents a partial
///     GraphQL response; consult the response envelope's <c>errors</c> collection before treating it as absent.
/// </remarks>
public sealed record GitLabWorkItemTypeConnection
{
    /// <summary>The selected work-item type nodes.</summary>
    [JsonPropertyName("nodes")]
    public IReadOnlyList<GitLabWorkItemType?>? Nodes { get; init; }

    /// <summary>The selected cursor metadata.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabWorkItemPageInfo? PageInfo { get; init; }
}