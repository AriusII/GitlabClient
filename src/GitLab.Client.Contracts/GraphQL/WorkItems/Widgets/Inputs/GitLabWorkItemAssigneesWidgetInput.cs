using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>assigneesWidget</c> work-item mutation argument.</summary>
/// <remarks>
///     Supply an empty list to clear every assignee. A null list is invalid because GitLab requires
///     <c>assigneeIds</c> whenever this widget input is supplied.
/// </remarks>
public sealed record GitLabWorkItemAssigneesWidgetInput
{
    /// <summary>The complete replacement set of assignee global IDs.</summary>
    [JsonPropertyName("assigneeIds")]
    public required IReadOnlyList<GitLabGraphQLGlobalId> AssigneeIds { get; init; }
}