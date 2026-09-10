namespace GitLab.Client.Models;

/// <summary>
///     One entry of a milestone's burndown chart
///     (<c>GET /projects/:id/milestones/:milestone_id/burndown_events</c> and the group equivalent): an issue
///     being opened, closed or reopened against the milestone, and the weight it contributed.
///     <para>
///         Every member is optional because the pinned OpenAPI document declares this response with no
///         schema at all, so the shape is taken from GitLab's <c>BurndownEvent</c> entity rather than from
///         the spec. Missing fields must degrade to <c>null</c>, not to a deserialization failure.
///     </para>
/// </summary>
public sealed record GitLabBurndownEvent
{
    /// <summary>When the event happened.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The issue weight the event added to or removed from the milestone.</summary>
    public int? Weight { get; init; }

    /// <summary>What happened - <c>"created"</c>, <c>"closed"</c> or <c>"reopened"</c>.</summary>
    public string? Action { get; init; }
}