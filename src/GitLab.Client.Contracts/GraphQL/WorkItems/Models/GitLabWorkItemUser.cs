using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The intentionally small user projection used by the curated work-item widgets.</summary>
/// <remarks>
///     GraphQL can preserve this object while reporting a field-level resolver error. Every response member is
///     nullable so the response envelope can retain both partial <c>data</c> and its top-level errors.
/// </remarks>
public sealed record GitLabWorkItemUser
{
    /// <summary>The user's opaque GraphQL global ID, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The user's display name, when resolved.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The user's handle when it was selected.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>The user's web URL when it was selected.</summary>
    [JsonPropertyName("webUrl")]
    public Uri? WebUrl { get; init; }
}