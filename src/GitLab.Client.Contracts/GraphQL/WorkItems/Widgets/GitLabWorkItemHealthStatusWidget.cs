using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The health-status fragment of a work item returned by <c>WorkItemWidgetHealthStatus</c>.</summary>
/// <remarks>
///     A null <see cref="HealthStatus" /> means that the selected widget has no health status, its field was not
///     selected, or its resolver failed. Consult the enclosing response's <c>errors</c> collection to distinguish a
///     resolver failure. It does not mean the health-status widget is unsupported; the entire fragment is absent in
///     that case.
/// </remarks>
public sealed record GitLabWorkItemHealthStatusWidget : GitLabWorkItemWidget
{
    /// <summary>The configured health status, or null when the work item has none.</summary>
    [JsonPropertyName("healthStatus")]
    public GitLabWorkItemHealthStatus? HealthStatus { get; init; }
}