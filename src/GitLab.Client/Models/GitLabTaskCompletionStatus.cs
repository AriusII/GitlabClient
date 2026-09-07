namespace GitLab.Client.Models;

/// <summary>
///     How far through its task list an issuable is (<c>APIEntitiesTaskCompletionStatus</c>), embedded as
///     <c>task_completion_status</c> in an issue or a merge request.
/// </summary>
public sealed record GitLabTaskCompletionStatus
{
    /// <summary>The number of checkbox tasks in the description.</summary>
    public int? Count { get; init; }

    /// <summary>How many of them are ticked.</summary>
    public int? CompletedCount { get; init; }
}