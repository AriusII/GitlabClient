namespace GitLab.Client.Models;

/// <summary>
///     A milestone being assigned to or removed from an issue or merge request, from that resource's event
///     history (<c>/projects/:id/issues/:issue_iid/resource_milestone_events</c>).
/// </summary>
public sealed record GitLabResourceMilestoneEvent
{
    public required long Id { get; init; }

    /// <summary>Who performed the change.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>What the event is attached to.</summary>
    public string? ResourceType { get; init; }

    /// <summary>The internal (non-iid) id of the issue or merge request the event belongs to.</summary>
    public long? ResourceId { get; init; }

    /// <summary>The milestone involved. Its own <c>active</c>/<c>closed</c> state lives on this object.</summary>
    public GitLabMilestone? Milestone { get; init; }

    /// <summary>
    ///     Whether the milestone was attached or detached. This family is the only one carrying both an
    ///     action and a <see cref="State" />.
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    ///     The state of the <i>issuable</i> at the time of the event - not the milestone's state, which is
    ///     <c>Milestone.State</c>. See the GitLab docs for why the two are easy to
    ///     confuse.
    /// </summary>
    public string? State { get; init; }
}