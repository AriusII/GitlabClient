using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The shallow hierarchy fragment of a work item returned by <c>WorkItemWidgetHierarchy</c>.</summary>
/// <remarks>
///     Parent, child, and ancestor relations deliberately use <see cref="GitLabWorkItemReference" /> rather than
///     recursively embedding <see cref="GitLabWorkItem" />. Empty child or ancestor connections are meaningful only
///     when their connection member is non-null; a null member also represents an omitted or failed selection.
/// </remarks>
public sealed record GitLabWorkItemHierarchyWidget : GitLabWorkItemWidget
{
    /// <summary>The immediate parent reference, or null when no parent is assigned or the field is unavailable.</summary>
    [JsonPropertyName("parent")]
    public GitLabWorkItemReference? Parent { get; init; }

    /// <summary>The cursor-paginated immediate-child references, when selected.</summary>
    [JsonPropertyName("children")]
    public GitLabWorkItemReferenceConnection? Children { get; init; }

    /// <summary>The cursor-paginated ancestor references, when selected.</summary>
    [JsonPropertyName("ancestors")]
    public GitLabWorkItemReferenceConnection? Ancestors { get; init; }

    /// <summary>Whether GitLab reports an immediate child, when selected and resolved.</summary>
    [JsonPropertyName("hasChildren")]
    public bool? HasChildren { get; init; }

    /// <summary>Whether GitLab reports an immediate parent, when selected and resolved.</summary>
    [JsonPropertyName("hasParent")]
    public bool? HasParent { get; init; }
}