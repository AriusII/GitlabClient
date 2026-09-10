namespace GitLab.Client.Models;

/// <summary>
///     An issue, merge request or epic being closed, reopened or merged, from that resource's event history
///     (<c>/projects/:id/issues/:issue_iid/resource_state_events</c>).
/// </summary>
public sealed record GitLabResourceStateEvent
{
    public required long Id { get; init; }

    /// <summary>Who performed the change.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>What the event is attached to.</summary>
    public string? ResourceType { get; init; }

    /// <summary>The internal (non-iid) id of the issue, merge request or epic the event belongs to.</summary>
    public long? ResourceId { get; init; }

    /// <summary>SHA of the commit that closed the resource, when a commit message triggered the change.</summary>
    public string? SourceCommit { get; init; }

    /// <summary>Id of the merge request that closed the resource, when merging triggered the change.</summary>
    public long? SourceMergeRequestId { get; init; }

    /// <summary>The state the resource moved to.</summary>
    public string? State { get; init; }
}