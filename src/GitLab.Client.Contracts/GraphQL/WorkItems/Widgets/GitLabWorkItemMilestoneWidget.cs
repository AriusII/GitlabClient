using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The milestone fragment of a work item returned by <c>WorkItemWidgetMilestone</c>.</summary>
/// <remarks>
///     A null <see cref="Milestone" /> means the selected widget has no assigned milestone, the field was omitted,
///     or the field failed to resolve in a partial GraphQL response. It does not imply the widget is unsupported.
/// </remarks>
public sealed record GitLabWorkItemMilestoneWidget : GitLabWorkItemWidget
{
    /// <summary>The assigned milestone, when selected and resolved.</summary>
    [JsonPropertyName("milestone")]
    public GitLabWorkItemMilestone? Milestone { get; init; }
}