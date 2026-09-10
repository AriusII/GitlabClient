using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.Protocol;

/// <summary>One line-and-column location in a GraphQL document.</summary>
public sealed record GitLabGraphQLErrorLocation
{
    /// <summary>The one-based source line reported by GitLab.</summary>
    [JsonPropertyName("line")]
    public int Line { get; init; }

    /// <summary>The one-based source column reported by GitLab.</summary>
    [JsonPropertyName("column")]
    public int Column { get; init; }
}