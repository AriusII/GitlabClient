using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The description fragment of a work item returned by <c>WorkItemWidgetDescription</c>.</summary>
/// <remarks>
///     A null <see cref="Description" /> means that the selected widget has no description, was not selected, or
///     could not be resolved. The enclosing GraphQL response's <c>errors</c> collection disambiguates a resolver
///     failure from an unset value. The absence of this fragment itself means the description widget was not selected
///     or is unavailable for the work-item type.
/// </remarks>
public sealed record GitLabWorkItemDescriptionWidget : GitLabWorkItemWidget
{
    /// <summary>The plain-text description, or null when no description is set.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The rendered HTML description, or null when no description is set or it was not selected.</summary>
    [JsonPropertyName("descriptionHtml")]
    public string? DescriptionHtml { get; init; }
}