using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     Cursor pagination state for one <c>POST /glql</c> result. GLQL is backed by GraphQL, so these four
///     members keep their camelCase wire names rather than the snake_case the rest of the REST API uses.
/// </summary>
public sealed record GitLabGlqlPageInfo
{
    /// <summary>Cursor of the last item on this page - pass it as <see cref="ExecuteGlqlQueryRequest.After" />.</summary>
    [JsonPropertyName("endCursor")]
    public string? EndCursor { get; init; }

    /// <summary>Whether another page follows.</summary>
    [JsonPropertyName("hasNextPage")]
    public bool? HasNextPage { get; init; }

    /// <summary>Whether a page precedes this one.</summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool? HasPreviousPage { get; init; }

    /// <summary>Cursor of the first item on this page.</summary>
    [JsonPropertyName("startCursor")]
    public string? StartCursor { get; init; }
}