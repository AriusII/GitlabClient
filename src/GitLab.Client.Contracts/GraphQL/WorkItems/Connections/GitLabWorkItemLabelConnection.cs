using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>A cursor-paginated GraphQL connection of labels assigned to a work item.</summary>
/// <remarks>
///     An empty <see cref="Nodes" /> collection means the selected labels connection has no labels. A null collection
///     means the field was omitted, inaccessible, or unresolved in a partial GraphQL response.
/// </remarks>
public sealed record GitLabWorkItemLabelConnection
{
    /// <summary>The selected label nodes. A null element denotes a partial GraphQL node result.</summary>
    [JsonPropertyName("nodes")]
    public IReadOnlyList<GitLabWorkItemLabel?>? Nodes { get; init; }

    /// <summary>The selected cursor metadata.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabWorkItemPageInfo? PageInfo { get; init; }
}