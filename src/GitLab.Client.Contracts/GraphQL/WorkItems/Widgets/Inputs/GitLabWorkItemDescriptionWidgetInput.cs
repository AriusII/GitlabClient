using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>descriptionWidget</c> work-item mutation argument.</summary>
public sealed record GitLabWorkItemDescriptionWidgetInput
{
    /// <summary>The replacement description. Use an empty string to clear the description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}