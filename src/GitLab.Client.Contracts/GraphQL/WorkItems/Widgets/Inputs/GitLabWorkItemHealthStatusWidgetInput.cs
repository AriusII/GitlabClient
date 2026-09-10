using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>healthStatusWidget</c> work-item mutation argument.</summary>
public sealed record GitLabWorkItemHealthStatusWidgetInput
{
    /// <summary>The health-status value to apply.</summary>
    [JsonPropertyName("healthStatus")]
    public required GitLabWorkItemHealthStatus HealthStatus { get; init; }
}