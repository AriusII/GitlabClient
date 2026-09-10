using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>A work-item type made available in a GitLab namespace.</summary>
/// <remarks>
///     Type IDs are namespace-specific. Resolve and retain the <see cref="Id" /> returned by the target namespace
///     instead of assuming that an Epic, Issue, or custom type has a stable ID across GitLab instances.
///     GraphQL can preserve this object while reporting a field-level resolver error, so every response member is
///     nullable and must be considered alongside the enclosing response's <c>errors</c> collection.
/// </remarks>
public sealed record GitLabWorkItemType
{
    /// <summary>The opaque GraphQL ID required by <c>workItemCreate</c>, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The display name configured for this work-item type, when resolved.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The system base type when GitLab exposes it; null for a selection that did not request it.</summary>
    [JsonPropertyName("baseType")]
    public string? BaseType { get; init; }

    /// <summary>The GitLab icon name when it was selected; null if absent from the GraphQL selection.</summary>
    [JsonPropertyName("iconName")]
    public string? IconName { get; init; }
}