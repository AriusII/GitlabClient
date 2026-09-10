using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Models;

namespace GitLab.Client.Composition.WorkItems;

/// <summary>The normalized values of the curated GraphQL work-item widgets.</summary>
/// <remarks>
///     A <c>Has…Widget</c> flag is the presence of the polymorphic GraphQL fragment, not a claim that the contained
///     value is set. For example, <see cref="HasDescriptionWidget" /> can be true while <see cref="Description" /> is
///     null, and <see cref="HasAssigneesWidget" /> can be true while <see cref="Assignees" /> is an empty list.
///     Unselected or unavailable widgets keep their values null and their flag false.
/// </remarks>
public sealed record GitLabWorkItemWidgetComposition
{
    /// <summary>Whether the description-widget fragment was available.</summary>
    public required bool HasDescriptionWidget { get; init; }

    /// <summary>The normalized plain-text description from the description widget.</summary>
    public string? Description { get; init; }

    /// <summary>The normalized rendered HTML description from the description widget.</summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>Whether the assignees-widget fragment was available.</summary>
    public required bool HasAssigneesWidget { get; init; }

    /// <summary>
    ///     The assignee nodes supplied by the widget. An empty list is meaningful only when
    ///     <see cref="HasAssigneesWidget" /> is true; a null list means the nested connection was not selected or
    ///     available, and a null node preserves a partial GraphQL resolver result.
    /// </summary>
    public IReadOnlyList<GitLabWorkItemUser?>? Assignees { get; init; }

    /// <summary>Whether the health-status-widget fragment was available.</summary>
    public required bool HasHealthStatusWidget { get; init; }

    /// <summary>The normalized health status from the health-status widget.</summary>
    public GitLabWorkItemHealthStatus? HealthStatus { get; init; }

    /// <summary>Whether the start/due-date-widget fragment was available.</summary>
    public required bool HasStartAndDueDateWidget { get; init; }

    /// <summary>The normalized start date from the start/due-date widget.</summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>The normalized due date from the start/due-date widget.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>The normalized fixed-date flag from the start/due-date widget.</summary>
    public bool? DatesAreFixed { get; init; }

    /// <summary>Whether the color-widget fragment was available.</summary>
    public required bool HasColorWidget { get; init; }

    /// <summary>The normalized background color from the color widget.</summary>
    public string? Color { get; init; }

    /// <summary>The normalized foreground text color from the color widget.</summary>
    public string? TextColor { get; init; }

    /// <summary>Whether the labels-widget fragment was available.</summary>
    public required bool HasLabelsWidget { get; init; }

    /// <summary>
    ///     The direct GraphQL labels connection. Its cursor metadata and node-list nullability are retained; an
    ///     empty node list is distinct from a null connection and must not be replaced with REST labels.
    /// </summary>
    public GitLabWorkItemLabelConnection? Labels { get; init; }

    /// <summary>Whether the milestone-widget fragment was available.</summary>
    public required bool HasMilestoneWidget { get; init; }

    /// <summary>
    ///     The direct GraphQL milestone projection. A null value with <see cref="HasMilestoneWidget" /> true means
    ///     that no milestone is assigned or the selected field did not resolve; it is not a REST fallback.
    /// </summary>
    public GitLabWorkItemMilestone? Milestone { get; init; }

    /// <summary>Whether the iteration-widget fragment was available.</summary>
    public required bool HasIterationWidget { get; init; }

    /// <summary>
    ///     The direct GraphQL iteration projection. A null value with <see cref="HasIterationWidget" /> true is not
    ///     equivalent to an unavailable iteration widget and is never joined to a REST iteration ID.
    /// </summary>
    public GitLabWorkItemIteration? Iteration { get; init; }

    /// <summary>Whether the shallow hierarchy-widget fragment was available.</summary>
    public required bool HasHierarchyWidget { get; init; }

    /// <summary>The immediate GraphQL parent reference, when selected and resolved.</summary>
    public GitLabWorkItemReference? Parent { get; init; }

    /// <summary>
    ///     The direct-child connection. Its references are deliberately non-recursive and its page metadata is
    ///     retained unchanged.
    /// </summary>
    public GitLabWorkItemReferenceConnection? Children { get; init; }

    /// <summary>
    ///     The flat ancestor connection. Its references are deliberately non-recursive and its page metadata is
    ///     retained unchanged.
    /// </summary>
    public GitLabWorkItemReferenceConnection? Ancestors { get; init; }

    /// <summary>The direct-child availability flag selected by the hierarchy widget.</summary>
    public bool? HasChildren { get; init; }

    /// <summary>The direct-parent availability flag selected by the hierarchy widget.</summary>
    public bool? HasParent { get; init; }
}