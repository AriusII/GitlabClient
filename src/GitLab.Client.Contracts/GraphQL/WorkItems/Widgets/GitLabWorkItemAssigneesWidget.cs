using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Connections;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The assignees fragment of a work item returned by <c>WorkItemWidgetAssignees</c>.</summary>
/// <remarks>
///     An empty <see cref="Assignees" /> connection means the selected widget has no assignees. A null connection
///     means its connection field was not selected or could not be resolved; it is not equivalent to an empty list.
///     A resolver failure is recorded in the enclosing GraphQL response's <c>errors</c> collection.
/// </remarks>
public sealed record GitLabWorkItemAssigneesWidget : GitLabWorkItemWidget
{
    /// <summary>The cursor-paginated assignee connection.</summary>
    [JsonPropertyName("assignees")]
    public GitLabWorkItemUserConnection? Assignees { get; init; }
}