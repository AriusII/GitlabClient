using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>hierarchyWidget</c> argument of <c>workItemUpdate</c>.</summary>
/// <remarks>
///     <see cref="ParentId" /> has three distinct states: null with <see cref="RemoveParent" /> false omits the
///     field, a non-null value assigns a parent, and <see cref="RemoveParent" /> true sends <c>parentId: null</c> to
///     remove the association. The custom converter preserves this GraphQL distinction without reflection.
/// </remarks>
[JsonConverter(typeof(GitLabWorkItemHierarchyUpdateWidgetInputJsonConverter))]
public sealed record GitLabWorkItemHierarchyUpdateWidgetInput
{
    /// <summary>The replacement parent global ID, when assigning a parent.</summary>
    [JsonIgnore]
    public GitLabGraphQLGlobalId? ParentId { get; init; }

    /// <summary>Whether to serialize an explicit <c>parentId: null</c> and remove the current parent.</summary>
    [JsonIgnore]
    public bool RemoveParent { get; init; }

    /// <summary>The child work-item global IDs to assign, when supplied.</summary>
    [JsonIgnore]
    public IReadOnlyList<GitLabGraphQLGlobalId>? ChildrenIds { get; init; }

    /// <summary>The adjacent work-item global ID used with <see cref="RelativePosition" />, when supplied.</summary>
    [JsonIgnore]
    public GitLabGraphQLGlobalId? AdjacentWorkItemId { get; init; }

    /// <summary>The relative position to the adjacent work item, when supplied.</summary>
    [JsonIgnore]
    public GitLabWorkItemRelativePosition? RelativePosition { get; init; }
}