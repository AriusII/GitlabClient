using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

/// <summary>Input for the documented <c>startAndDueDateWidget</c> work-item mutation argument.</summary>
/// <remarks>
///     A null member is omitted by the library's JSON options, so this contract updates only the supplied date
///     members. A caller that needs a server-specific clear operation should use an explicit operation added for
///     that schema version rather than relying on null serialization semantics.
/// </remarks>
public sealed record GitLabWorkItemStartAndDueDateWidgetInput
{
    /// <summary>The replacement start date, when supplied.</summary>
    [JsonPropertyName("startDate")]
    public DateOnly? StartDate { get; init; }

    /// <summary>The replacement due date, when supplied.</summary>
    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; init; }

    /// <summary>Whether GitLab should retain fixed dates rather than roll them up, when supported and supplied.</summary>
    [JsonPropertyName("isFixed")]
    public bool? IsFixed { get; init; }
}