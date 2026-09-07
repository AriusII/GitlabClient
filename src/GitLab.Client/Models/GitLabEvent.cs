namespace GitLab.Client.Models;

/// <summary>
///     One entry from a GitLab activity feed (<c>/events</c>, <c>/projects/:id/events</c>,
///     <c>/users/:id/events</c>): who did what, to which target, and when.
///     <para>
///         Which of the optional payload members is populated depends on <see cref="TargetType" /> - a
///         comment carries <see cref="Note" />, a push carries <see cref="PushData" />, a wiki edit carries
///         <see cref="WikiPage" /> - so read the target type first.
///     </para>
/// </summary>
public sealed record GitLabEvent
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>What happened, in GitLab's own words: "pushed to", "opened", "commented on", "joined", ...</summary>
    public string? ActionName { get; init; }

    public long? TargetId { get; init; }

    /// <summary>The target's project-scoped iid, where it has one (issues and merge requests).</summary>
    public long? TargetIid { get; init; }

    /// <summary>
    ///     One of "issue", "milestone", "merge_request", "note", "project", "snippet", "user", "wiki" or
    ///     "design"; null for events with no target, such as a push.
    /// </summary>
    public string? TargetType { get; init; }

    public long? AuthorId { get; init; }

    public string? TargetTitle { get; init; }

    /// <remarks>
    ///     The spec declares this as a bare string with no <c>format</c>, unlike every other timestamp in the
    ///     document. GitLab does emit ISO-8601, and <c>EventsRepositoryTests</c> pins that against a recorded
    ///     payload.
    /// </remarks>
    public DateTimeOffset? CreatedAt { get; init; }

    public GitLabUser? Author { get; init; }

    public string? AuthorUsername { get; init; }

    /// <summary>The comment, when this event is a note event.</summary>
    public GitLabNote? Note { get; init; }

    /// <summary>The push details, when this event is a push event.</summary>
    public GitLabPushEventPayload? PushData { get; init; }

    /// <summary>The wiki page, when this event is a wiki event. Carries the basic shape only - no content.</summary>
    public GitLabWikiPage? WikiPage { get; init; }

    public bool? Imported { get; init; }

    public string? ImportedFrom { get; init; }
}