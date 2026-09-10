using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>iterationWidget</c> work-item mutation argument.</summary>
/// <remarks>
///     GitLab accepts an explicit <c>iterationId: null</c> to clear the assigned iteration. This contract therefore
///     deliberately writes a null <see cref="IterationId" /> instead of inheriting the context's null-omission rule.
/// </remarks>
public sealed record GitLabWorkItemIterationWidgetInput
{
    /// <summary>The iteration global ID to assign, or null to remove the assignment.</summary>
    [JsonPropertyName("iterationId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public GitLabGraphQLGlobalId? IterationId { get; init; }
}