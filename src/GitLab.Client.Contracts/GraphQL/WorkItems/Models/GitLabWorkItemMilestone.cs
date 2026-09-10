using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The shallow milestone projection returned by <c>WorkItemWidgetMilestone</c>.</summary>
/// <remarks>
///     This is intentionally a projection rather than the complete GitLab milestone resource. Every field is
///     nullable so a field-level GraphQL error does not discard the rest of the enclosing work-item data.
/// </remarks>
public sealed record GitLabWorkItemMilestone
{
    /// <summary>The milestone's opaque GraphQL global ID, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The milestone title, when resolved.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The milestone description, when selected and resolved.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The milestone start date, when selected and configured.</summary>
    [JsonPropertyName("startDate")]
    public DateOnly? StartDate { get; init; }

    /// <summary>The milestone due date, when selected and configured.</summary>
    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; init; }

    /// <summary>The milestone web URL, when selected and resolved.</summary>
    [JsonPropertyName("webUrl")]
    public Uri? WebUrl { get; init; }
}