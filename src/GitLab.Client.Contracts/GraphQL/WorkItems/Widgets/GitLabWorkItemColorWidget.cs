using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets;

/// <summary>The color fragment of a work item returned by <c>WorkItemWidgetColor</c>.</summary>
/// <remarks>
///     A null <see cref="Color" /> or <see cref="TextColor" /> denotes an unset value, a field omitted from the
///     GraphQL selection, or a resolver failure. Consult the enclosing response's <c>errors</c> collection to
///     distinguish the latter. The absence of this fragment means the color widget was not selected or is unavailable.
/// </remarks>
public sealed record GitLabWorkItemColorWidget : GitLabWorkItemWidget
{
    /// <summary>The configured color, typically a CSS hexadecimal color string.</summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>The GitLab-selected foreground color for text rendered against <see cref="Color" />.</summary>
    [JsonPropertyName("textColor")]
    public string? TextColor { get; init; }
}