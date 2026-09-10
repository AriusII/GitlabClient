namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /ai/duo_workflows/workflows/:id/checkpoint_writes_batch</c>.</summary>
public sealed record CreateDuoWorkflowCheckpointWritesRequest
{
    /// <summary>The LangGraph <c>thread_ts</c> of the checkpoint the writes belong to.</summary>
    public required string ThreadTs { get; init; }

    /// <summary>The writes to record, in one batch.</summary>
    public required IReadOnlyList<DuoWorkflowCheckpointWrite> CheckpointWrites { get; init; }
}