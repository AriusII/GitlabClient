using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The shallow iteration projection returned by <c>WorkItemWidgetIteration</c>.</summary>
/// <remarks>
///     Every member is nullable to retain any independently resolved fields when GraphQL also returns top-level
///     errors. Null is not interchangeable with an empty iteration or an unavailable widget.
/// </remarks>
public sealed record GitLabWorkItemIteration
{
    /// <summary>The iteration's opaque GraphQL global ID, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The iteration title, when resolved.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The iteration description, when selected and resolved.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The iteration start date, when selected and configured.</summary>
    [JsonPropertyName("startDate")]
    public DateOnly? StartDate { get; init; }

    /// <summary>The iteration due date, when selected and configured.</summary>
    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; init; }

    /// <summary>The iteration web URL, when selected and resolved.</summary>
    [JsonPropertyName("webUrl")]
    public Uri? WebUrl { get; init; }
}