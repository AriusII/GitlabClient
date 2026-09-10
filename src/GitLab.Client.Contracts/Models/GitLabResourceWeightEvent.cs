namespace GitLab.Client.Models;

/// <summary>
///     An issue's weight being set or cleared, from that issue's event history
///     (<c>/projects/:id/issues/:issue_iid/resource_weight_events</c>). Merge requests have no weight events.
/// </summary>
/// <remarks>
///     The odd one out of the five event families: GitLab sends <c>issue_id</c> here rather than the
///     <c>resource_type</c>/<c>resource_id</c> pair the others carry, so this type is deliberately not
///     normalised into their shape.
/// </remarks>
public sealed record GitLabResourceWeightEvent
{
    public required long Id { get; init; }

    /// <summary>Who performed the change.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The internal (non-iid) id of the issue the event belongs to.</summary>
    public long? IssueId { get; init; }

    /// <summary>The weight the issue was set to; null when the weight was cleared.</summary>
    public int? Weight { get; init; }
}