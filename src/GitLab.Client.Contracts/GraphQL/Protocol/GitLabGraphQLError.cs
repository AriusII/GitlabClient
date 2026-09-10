using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.Protocol;

/// <summary>One top-level GraphQL error reported by GitLab.</summary>
public sealed record GitLabGraphQLError
{
    /// <summary>The human-readable error message returned by GitLab.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>The document locations associated with the error, when GitLab supplies them.</summary>
    [JsonPropertyName("locations")]
    public IReadOnlyList<GitLabGraphQLErrorLocation>? Locations { get; init; }

    /// <summary>
    ///     The response path associated with the error. A GraphQL path can contain both property names and array
    ///     indices, so the complete JSON value is preserved rather than flattened into a lossy string list.
    /// </summary>
    [JsonPropertyName("path")]
    public JsonElement? Path { get; init; }

    /// <summary>
    ///     Error-specific extension data. Its schema depends on the GitLab operation and server configuration.
    /// </summary>
    [JsonPropertyName("extensions")]
    public JsonElement? Extensions { get; init; }
}