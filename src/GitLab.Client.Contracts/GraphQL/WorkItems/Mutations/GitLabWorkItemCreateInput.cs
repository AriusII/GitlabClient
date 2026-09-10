using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The curated input for the GitLab GraphQL <c>workItemCreate</c> mutation.</summary>
/// <remarks>
///     The optional widget inputs are accepted only when the selected work-item type supports the corresponding
///     widget. GitLab reports unsupported widgets through the mutation payload's <c>errors</c> collection.
/// </remarks>
public sealed record GitLabWorkItemCreateInput
{
    /// <summary>Initializes the immutable identity and title portion of a create operation.</summary>
    /// <param name="title">The non-empty title of the new work item.</param>
    /// <param name="namespacePath">The non-empty full path of the containing project or group namespace.</param>
    /// <param name="workItemTypeId">The namespace-specific global ID of the requested work-item type.</param>
    /// <exception cref="ArgumentException"><paramref name="title" /> or <paramref name="namespacePath" /> is blank.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="workItemTypeId" /> is null.</exception>
    public GitLabWorkItemCreateInput(string title, string namespacePath, GitLabGraphQLGlobalId workItemTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(namespacePath);
        ArgumentNullException.ThrowIfNull(workItemTypeId);

        Title = title;
        NamespacePath = namespacePath;
        WorkItemTypeId = workItemTypeId;
    }

    /// <summary>The title of the new work item.</summary>
    [JsonPropertyName("title")]
    public string Title { get; }

    /// <summary>The full path of the project or group namespace in which to create the work item.</summary>
    [JsonPropertyName("namespacePath")]
    public string NamespacePath { get; }

    /// <summary>The namespace-specific global ID of the work-item type to create.</summary>
    [JsonPropertyName("workItemTypeId")]
    public GitLabGraphQLGlobalId WorkItemTypeId { get; }

    /// <summary>Whether GitLab should mark the new work item confidential.</summary>
    [JsonPropertyName("confidential")]
    public bool? Confidential { get; init; }

    /// <summary>The optional description-widget input.</summary>
    [JsonPropertyName("descriptionWidget")]
    public GitLabWorkItemDescriptionWidgetInput? DescriptionWidget { get; init; }

    /// <summary>The optional assignees-widget input.</summary>
    [JsonPropertyName("assigneesWidget")]
    public GitLabWorkItemAssigneesWidgetInput? AssigneesWidget { get; init; }

    /// <summary>The optional color-widget input.</summary>
    [JsonPropertyName("colorWidget")]
    public GitLabWorkItemColorWidgetInput? ColorWidget { get; init; }

    /// <summary>The optional health-status-widget input.</summary>
    [JsonPropertyName("healthStatusWidget")]
    public GitLabWorkItemHealthStatusWidgetInput? HealthStatusWidget { get; init; }

    /// <summary>The optional start/due-date-widget input.</summary>
    [JsonPropertyName("startAndDueDateWidget")]
    public GitLabWorkItemStartAndDueDateWidgetInput? StartAndDueDateWidget { get; init; }

    /// <summary>The optional initial labels-widget input.</summary>
    [JsonPropertyName("labelsWidget")]
    public GitLabWorkItemLabelsCreateWidgetInput? LabelsWidget { get; init; }

    /// <summary>The optional milestone-widget input.</summary>
    [JsonPropertyName("milestoneWidget")]
    public GitLabWorkItemMilestoneWidgetInput? MilestoneWidget { get; init; }

    /// <summary>The optional iteration-widget input.</summary>
    [JsonPropertyName("iterationWidget")]
    public GitLabWorkItemIterationWidgetInput? IterationWidget { get; init; }

    /// <summary>The optional initial hierarchy-widget input.</summary>
    [JsonPropertyName("hierarchyWidget")]
    public GitLabWorkItemHierarchyCreateWidgetInput? HierarchyWidget { get; init; }
}