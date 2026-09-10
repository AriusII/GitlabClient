using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The closed polymorphic union of work-item widgets covered by the curated GraphQL operations.</summary>
/// <remarks>
///     This abstract record is intentionally the one non-sealed contract in this folder: System.Text.Json needs a
///     common declared type to deserialize the GraphQL <c>widgets</c> union. The discriminator list is deliberately
///     closed to the curated widgets modelled by this SDK. Curated operation documents must request
///     <c>__typename</c> and restrict <c>widgets</c> to those types; unsupported custom-field, AI, custom-status, or
///     bulk widgets are not silently materialized as loosely typed JSON. A selected widget can still be null in a
///     partial GraphQL result; the containing <see cref="GitLabWorkItem.Widgets" /> collection models that case.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "__typename")]
[JsonDerivedType(typeof(GitLabWorkItemDescriptionWidget), "WorkItemWidgetDescription")]
[JsonDerivedType(typeof(GitLabWorkItemAssigneesWidget), "WorkItemWidgetAssignees")]
[JsonDerivedType(typeof(GitLabWorkItemColorWidget), "WorkItemWidgetColor")]
[JsonDerivedType(typeof(GitLabWorkItemHealthStatusWidget), "WorkItemWidgetHealthStatus")]
[JsonDerivedType(typeof(GitLabWorkItemStartAndDueDateWidget), "WorkItemWidgetStartAndDueDate")]
[JsonDerivedType(typeof(GitLabWorkItemLabelsWidget), "WorkItemWidgetLabels")]
[JsonDerivedType(typeof(GitLabWorkItemMilestoneWidget), "WorkItemWidgetMilestone")]
[JsonDerivedType(typeof(GitLabWorkItemIterationWidget), "WorkItemWidgetIteration")]
[JsonDerivedType(typeof(GitLabWorkItemHierarchyWidget), "WorkItemWidgetHierarchy")]
public abstract record GitLabWorkItemWidget;