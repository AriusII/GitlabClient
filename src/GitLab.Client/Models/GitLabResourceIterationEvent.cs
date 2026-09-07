namespace GitLab.Client.Models;

/// <summary>
///     An iteration being assigned to or removed from an issue, from that issue's event history
///     (<c>/projects/:id/issues/:issue_iid/resource_iteration_events</c>). Merge requests have no
///     iteration events.
/// </summary>
public sealed record GitLabResourceIterationEvent
{
    public required long Id { get; init; }

    /// <summary>Who performed the change.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>What the event is attached to - always <c>Issue</c>.</summary>
    public string? ResourceType { get; init; }

    /// <summary>The internal (non-iid) id of the issue the event belongs to.</summary>
    public long? ResourceId { get; init; }

    /// <summary>The iteration involved.</summary>
    public GitLabIteration? Iteration { get; init; }

    /// <summary>Whether the iteration was attached or detached.</summary>
    public string? Action { get; init; }
}