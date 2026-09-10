using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>milestoneWidget</c> work-item mutation argument.</summary>
/// <remarks>
///     GitLab accepts an explicit <c>milestoneId: null</c> to clear the assigned milestone. This contract therefore
///     deliberately writes a null <see cref="MilestoneId" /> instead of inheriting the context's null-omission rule.
/// </remarks>
public sealed record GitLabWorkItemMilestoneWidgetInput
{
    /// <summary>The milestone global ID to assign, or null to remove the assignment.</summary>
    [JsonPropertyName("milestoneId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public GitLabGraphQLGlobalId? MilestoneId { get; init; }
}