using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Connections;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The labels fragment of a work item returned by <c>WorkItemWidgetLabels</c>.</summary>
/// <remarks>
///     A selected empty <see cref="Labels" /> connection means no labels are assigned. A null connection means the
///     connection was not selected, is inaccessible, or could not be resolved in a partial GraphQL response.
/// </remarks>
public sealed record GitLabWorkItemLabelsWidget : GitLabWorkItemWidget
{
    /// <summary>The cursor-paginated label connection.</summary>
    [JsonPropertyName("labels")]
    public GitLabWorkItemLabelConnection? Labels { get; init; }
}