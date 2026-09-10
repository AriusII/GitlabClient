using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>labelsWidget</c> argument of <c>workItemUpdate</c>.</summary>
/// <remarks>
///     Unlike creation, updates are delta based. Null collections are omitted from the GraphQL input; an empty
///     collection is an explicit empty add or remove operation and is not equivalent to an unselected widget.
/// </remarks>
public sealed record GitLabWorkItemLabelsUpdateWidgetInput
{
    /// <summary>The label global IDs to add, when supplied.</summary>
    [JsonPropertyName("addLabelIds")]
    public IReadOnlyList<GitLabGraphQLGlobalId>? AddLabelIds { get; init; }

    /// <summary>The label global IDs to remove, when supplied.</summary>
    [JsonPropertyName("removeLabelIds")]
    public IReadOnlyList<GitLabGraphQLGlobalId>? RemoveLabelIds { get; init; }
}