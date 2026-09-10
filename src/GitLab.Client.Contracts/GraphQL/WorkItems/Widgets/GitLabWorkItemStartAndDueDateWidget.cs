using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The start/due-date fragment returned by <c>WorkItemWidgetStartAndDueDate</c>.</summary>
/// <remarks>
///     Each date is independently nullable: a work item can have only a start date, only a due date, or neither.
///     A null member can also arise from an omitted selection or a resolver failure, which the enclosing response's
///     <c>errors</c> collection records. The absence of this fragment means the widget was not selected or is
///     unavailable for this work-item type.
/// </remarks>
public sealed record GitLabWorkItemStartAndDueDateWidget : GitLabWorkItemWidget
{
    /// <summary>The calendar start date, or null when no start date is configured.</summary>
    [JsonPropertyName("startDate")]
    public DateOnly? StartDate { get; init; }

    /// <summary>The calendar due date, or null when no due date is configured.</summary>
    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; init; }

    /// <summary>
    ///     Whether GitLab keeps dates fixed rather than rolling them up, or null when the field was not selected or
    ///     the instance does not expose this tier-specific field.
    /// </summary>
    [JsonPropertyName("isFixed")]
    public bool? IsFixed { get; init; }
}