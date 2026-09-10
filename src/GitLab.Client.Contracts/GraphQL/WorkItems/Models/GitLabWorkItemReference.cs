using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>A deliberately non-recursive work-item projection for hierarchy links.</summary>
/// <remarks>
///     Parent, child, and ancestor links use this reference instead of <see cref="GitLabWorkItem" /> so a hierarchy
///     query cannot accidentally materialize an unbounded graph. Every member remains nullable for GraphQL partial
///     response semantics.
/// </remarks>
public sealed record GitLabWorkItemReference
{
    /// <summary>The linked work item's opaque GraphQL global ID, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The linked work item's namespace-local IID, when selected and resolved.</summary>
    [JsonPropertyName("iid")]
    public string? Iid { get; init; }

    /// <summary>The linked work item's title, when selected and resolved.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The linked work item's lifecycle state, when selected and resolved.</summary>
    [JsonPropertyName("state")]
    public GitLabWorkItemState? State { get; init; }

    /// <summary>The linked work item's type, when selected and resolved.</summary>
    [JsonPropertyName("workItemType")]
    public GitLabWorkItemType? WorkItemType { get; init; }

    /// <summary>The linked work item's web URL, when selected and resolved.</summary>
    [JsonPropertyName("webUrl")]
    public Uri? WebUrl { get; init; }
}