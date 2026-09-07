using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of a checkpoint write batch
///     (<c>POST /ai/duo_workflows/workflows/:id/checkpoint_writes_batch</c>).
/// </summary>
public sealed record DuoWorkflowCheckpointWrite
{
    /// <summary>The LangGraph task id the write belongs to.</summary>
    [JsonPropertyName("task")]
    public required string TaskId { get; init; }

    /// <summary>The write's position within the task.</summary>
    [JsonPropertyName("idx")]
    public required int Index { get; init; }

    /// <summary>The channel written to.</summary>
    public required string Channel { get; init; }

    /// <summary>The serialization type of <see cref="Data" />.</summary>
    public required string WriteType { get; init; }

    /// <summary>The written payload.</summary>
    public required string Data { get; init; }
}