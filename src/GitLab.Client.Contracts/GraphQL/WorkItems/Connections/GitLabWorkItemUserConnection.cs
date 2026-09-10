using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>A cursor-paginated GraphQL connection of the users assigned to a work item.</summary>
/// <remarks>
///     An empty <see cref="Nodes" /> collection means the selected assignee connection has no users. A null
///     collection means the fields were not selected or the enclosing widget was unavailable. A null node represents
///     a partial GraphQL response; consult the response envelope's <c>errors</c> collection before treating it as
///     absent.
/// </remarks>
public sealed record GitLabWorkItemUserConnection
{
    /// <summary>The selected assignee user nodes.</summary>
    [JsonPropertyName("nodes")]
    public IReadOnlyList<GitLabWorkItemUser?>? Nodes { get; init; }

    /// <summary>The selected cursor metadata.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabWorkItemPageInfo? PageInfo { get; init; }
}