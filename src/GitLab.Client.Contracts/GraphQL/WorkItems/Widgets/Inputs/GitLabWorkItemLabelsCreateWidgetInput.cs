using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>labelsWidget</c> argument of <c>workItemCreate</c>.</summary>
/// <remarks>
///     GitLab requires <c>labelIds</c> when this input is supplied. An empty list is therefore the explicit request
///     to create the work item with no labels, rather than a missing labels-widget selection.
/// </remarks>
public sealed record GitLabWorkItemLabelsCreateWidgetInput
{
    /// <summary>The complete initial set of label global IDs.</summary>
    [JsonPropertyName("labelIds")]
    public required IReadOnlyList<GitLabGraphQLGlobalId> LabelIds { get; init; }
}