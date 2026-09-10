using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The curated input for the GitLab GraphQL <c>workItemUpdate</c> mutation.</summary>
/// <remarks>
///     This contract intentionally excludes custom status, AI, bulk, and custom-field operations. Those surfaces are
///     feature- and tier-dependent and need an explicit versioned contract when they become stable.
/// </remarks>
public sealed record GitLabWorkItemUpdateInput
{
    /// <summary>Initializes an update for one opaque work-item global ID.</summary>
    /// <param name="id">The target work item's GraphQL global ID.</param>
    /// <exception cref="ArgumentNullException"><paramref name="id" /> is null.</exception>
    public GitLabWorkItemUpdateInput(GitLabGraphQLGlobalId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    /// <summary>The opaque GraphQL global ID of the work item to update.</summary>
    [JsonPropertyName("id")]
    public GitLabGraphQLGlobalId Id { get; }

    /// <summary>The replacement title, when supplied.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The replacement confidentiality flag, when supplied.</summary>
    [JsonPropertyName("confidential")]
    public bool? Confidential { get; init; }

    /// <summary>The optional description-widget update.</summary>
    [JsonPropertyName("descriptionWidget")]
    public GitLabWorkItemDescriptionWidgetInput? DescriptionWidget { get; init; }

    /// <summary>The optional assignees-widget update.</summary>
    [JsonPropertyName("assigneesWidget")]
    public GitLabWorkItemAssigneesWidgetInput? AssigneesWidget { get; init; }

    /// <summary>The optional color-widget update.</summary>
    [JsonPropertyName("colorWidget")]
    public GitLabWorkItemColorWidgetInput? ColorWidget { get; init; }

    /// <summary>The optional health-status-widget update.</summary>
    [JsonPropertyName("healthStatusWidget")]
    public GitLabWorkItemHealthStatusWidgetInput? HealthStatusWidget { get; init; }

    /// <summary>The optional start/due-date-widget update.</summary>
    [JsonPropertyName("startAndDueDateWidget")]
    public GitLabWorkItemStartAndDueDateWidgetInput? StartAndDueDateWidget { get; init; }

    /// <summary>The optional delta labels-widget update.</summary>
    [JsonPropertyName("labelsWidget")]
    public GitLabWorkItemLabelsUpdateWidgetInput? LabelsWidget { get; init; }

    /// <summary>The optional milestone-widget update.</summary>
    [JsonPropertyName("milestoneWidget")]
    public GitLabWorkItemMilestoneWidgetInput? MilestoneWidget { get; init; }

    /// <summary>The optional iteration-widget update.</summary>
    [JsonPropertyName("iterationWidget")]
    public GitLabWorkItemIterationWidgetInput? IterationWidget { get; init; }

    /// <summary>The optional hierarchy-widget update.</summary>
    [JsonPropertyName("hierarchyWidget")]
    public GitLabWorkItemHierarchyUpdateWidgetInput? HierarchyWidget { get; init; }
}