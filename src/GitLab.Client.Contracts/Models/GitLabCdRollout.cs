namespace GitLab.Client.Models;

/// <summary>The answer to <c>POST /rollouts/:id</c>: the CD rollout the ingested event updated.</summary>
public sealed record GitLabCdRollout
{
    public long? Id { get; init; }

    public long? Iid { get; init; }

    /// <summary>The rollout's current state, for example <c>"in_progress"</c>.</summary>
    public string? State { get; init; }

    public string? WorkflowRef { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }
}