namespace GitLab.Client.Models;

/// <summary>
///     A label being added to or removed from an issue, merge request or epic, from that resource's event
///     history (<c>/projects/:id/issues/:issue_iid/resource_label_events</c>).
/// </summary>
public sealed record GitLabResourceLabelEvent
{
    public required long Id { get; init; }

    /// <summary>Who performed the change.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>What the event is attached to.</summary>
    public string? ResourceType { get; init; }

    /// <summary>The internal (non-iid) id of the issue, merge request or epic the event belongs to.</summary>
    public long? ResourceId { get; init; }

    /// <summary>
    ///     The label involved. Null once the label itself has been deleted - GitLab keeps the event and drops
    ///     the label.
    /// </summary>
    public GitLabLabel? Label { get; init; }

    /// <summary>Whether the label was attached or detached.</summary>
    public string? Action { get; init; }
}