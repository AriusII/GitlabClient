using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The shallow label projection returned by <c>WorkItemWidgetLabels</c>.</summary>
/// <remarks>
///     Every member is nullable to preserve a GraphQL partial result together with its top-level errors. A null value
///     can therefore mean an omitted selection, an inaccessible field, or a resolver failure.
/// </remarks>
public sealed record GitLabWorkItemLabel
{
    /// <summary>The label's opaque GraphQL global ID, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The label title, when resolved.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The label description, when selected and resolved.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The label background color, when selected and resolved.</summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>The GitLab-selected foreground color, when selected and resolved.</summary>
    [JsonPropertyName("textColor")]
    public string? TextColor { get; init; }
}