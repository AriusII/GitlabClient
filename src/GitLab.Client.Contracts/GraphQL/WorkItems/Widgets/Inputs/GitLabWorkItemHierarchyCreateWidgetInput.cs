using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>hierarchyWidget</c> argument of <c>workItemCreate</c>.</summary>
public sealed record GitLabWorkItemHierarchyCreateWidgetInput
{
    /// <summary>The parent work-item global ID, when the new item should be created below an existing parent.</summary>
    [JsonPropertyName("parentId")]
    public GitLabGraphQLGlobalId? ParentId { get; init; }
}