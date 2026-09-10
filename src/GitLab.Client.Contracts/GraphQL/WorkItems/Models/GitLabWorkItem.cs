using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Widgets;

namespace GitLab.Client.GraphQL.WorkItems.Models;

/// <summary>The curated, non-widget projection of a GitLab GraphQL work item.</summary>
/// <remarks>
///     Widget data is deliberately represented by the separate fragment contracts in <c>Widgets</c>. GitLab selects
///     widgets per work-item type and entitlement, so a missing widget must not be mistaken for a core property with
///     an empty value. GraphQL can also return a partial <c>data</c> tree together with top-level errors. Therefore,
///     every selected member is nullable: null can mean an omitted selection, an unavailable value, or a resolver
///     failure recorded in the enclosing response's <c>errors</c> collection.
///     This contract is supported by this SDK's curated documents; it does not claim that GitLab's versionless Work
///     Item schema is stable across all instances or feature configurations.
/// </remarks>
public sealed record GitLabWorkItem
{
    /// <summary>The opaque GraphQL global ID of this work item, when resolved.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId? Id { get; init; }

    /// <summary>The namespace-local internal ID emitted as a string by GraphQL, when resolved.</summary>
    [JsonPropertyName("iid")]
    public string? Iid { get; init; }

    /// <summary>The work item's title, when resolved.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The open or closed state, when resolved.</summary>
    [JsonPropertyName("state")]
    public GitLabWorkItemState? State { get; init; }

    /// <summary>Whether GitLab marks the work item confidential, when resolved.</summary>
    [JsonPropertyName("confidential")]
    public bool? Confidential { get; init; }

    /// <summary>The plain-text description, or null when unset, not selected, or not resolved.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The rendered HTML description, or null when unset, not selected, or not resolved.</summary>
    [JsonPropertyName("descriptionHtml")]
    public string? DescriptionHtml { get; init; }

    /// <summary>The time at which GitLab created the work item, when selected and resolved.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The time at which GitLab last updated the work item, when selected and resolved.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The close time, or null when open, not selected, or not resolved.</summary>
    [JsonPropertyName("closedAt")]
    public DateTimeOffset? ClosedAt { get; init; }

    /// <summary>The author projection, or null when not selected, not visible, or not resolved.</summary>
    [JsonPropertyName("author")]
    public GitLabWorkItemUser? Author { get; init; }

    /// <summary>The configured work-item type, or null when not selected or not resolved.</summary>
    [JsonPropertyName("workItemType")]
    public GitLabWorkItemType? WorkItemType { get; init; }

    /// <summary>
    ///     The selected supported widget fragments. An empty list means the query selected no supported widgets for
    ///     this item; null means <c>widgets</c> was not selected or GitLab did not expose the field. A null element
    ///     represents a partial GraphQL result for a selected widget. Non-null elements are one of the closed
    ///     <see cref="GitLabWorkItemWidget" /> variants.
    /// </summary>
    [JsonPropertyName("widgets")]
    public IReadOnlyList<GitLabWorkItemWidget?>? Widgets { get; init; }

    /// <summary>The work item's GitLab web URL, when selected and resolved.</summary>
    [JsonPropertyName("webUrl")]
    public Uri? WebUrl { get; init; }
}