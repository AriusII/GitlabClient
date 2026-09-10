using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Connections;

/// <summary>Cursor pagination metadata emitted by GitLab GraphQL connections.</summary>
/// <remarks>
///     Each member is nullable because GraphQL only returns fields explicitly selected by the operation. For a
///     curated connection query, a null member indicates an incomplete response rather than a false or empty value.
/// </remarks>
public sealed record GitLabWorkItemPageInfo
{
    /// <summary>Whether another page follows the current page.</summary>
    [JsonPropertyName("hasNextPage")]
    public bool? HasNextPage { get; init; }

    /// <summary>Whether a page precedes the current page.</summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool? HasPreviousPage { get; init; }

    /// <summary>The cursor of the first result, or null for an empty page or an omitted field.</summary>
    [JsonPropertyName("startCursor")]
    public string? StartCursor { get; init; }

    /// <summary>The cursor of the last result, or null for an empty page or an omitted field.</summary>
    [JsonPropertyName("endCursor")]
    public string? EndCursor { get; init; }
}