using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>A cursor-paginated, non-recursive GraphQL connection of hierarchy work-item references.</summary>
/// <remarks>
///     An empty <see cref="Nodes" /> collection means the selected relation has no links. A null collection means the
///     connection was not selected, is unavailable for the work-item type, or was unresolved in a partial response.
/// </remarks>
public sealed record GitLabWorkItemReferenceConnection
{
    /// <summary>The selected parent, child, or ancestor references. A null element denotes a partial node result.</summary>
    [JsonPropertyName("nodes")]
    public IReadOnlyList<GitLabWorkItemReference?>? Nodes { get; init; }

    /// <summary>The selected cursor metadata.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabWorkItemPageInfo? PageInfo { get; init; }
}