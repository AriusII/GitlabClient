using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The iteration fragment of a work item returned by <c>WorkItemWidgetIteration</c>.</summary>
/// <remarks>
///     A null <see cref="Iteration" /> means the selected widget has no assigned iteration, the field was omitted,
///     or the field failed to resolve in a partial GraphQL response. It does not imply the widget is unsupported.
/// </remarks>
public sealed record GitLabWorkItemIterationWidget : GitLabWorkItemWidget
{
    /// <summary>The assigned iteration, when selected and resolved.</summary>
    [JsonPropertyName("iteration")]
    public GitLabWorkItemIteration? Iteration { get; init; }
}