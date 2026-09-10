using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of the <c>additional_context</c> array a flow is started with. GitLab spells both wire
///     names with a leading capital, which is why they are pinned with
///     <see cref="JsonPropertyNameAttribute" /> rather than left to the context's snake_case policy.
/// </summary>
public sealed record DuoWorkflowAdditionalContext
{
    /// <summary>The category of the context detail.</summary>
    [JsonPropertyName("Category")]
    public required string Category { get; init; }

    /// <summary>The content of the context detail.</summary>
    [JsonPropertyName("Content")]
    public required string Content { get; init; }
}