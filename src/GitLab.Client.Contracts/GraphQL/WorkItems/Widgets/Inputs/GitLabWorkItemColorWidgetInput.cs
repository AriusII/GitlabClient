using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>colorWidget</c> work-item mutation argument.</summary>
public sealed record GitLabWorkItemColorWidgetInput
{
    /// <summary>The replacement color accepted by the GitLab work-item color widget.</summary>
    [JsonPropertyName("color")]
    public required string Color { get; init; }
}