using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the GitLab activity feeds (<c>GET /events</c>, <c>GET /projects/:id/events</c>,
///     <c>GET /users/:id/events</c>). All three take the same parameter set, except that
///     <see cref="Scope" /> is honoured only by the instance feed.
/// </summary>
[GitLabQuery]
public readonly record struct EventListOptions
{
    /// <summary>
    ///     Widens the instance feed beyond the default - "all" for every scope. Honoured only by
    ///     <c>GET /events</c>; the project and user feeds ignore it. Left as free text because the spec
    ///     declares no vocabulary for it, unlike the filters below.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>Include only events of this action type.</summary>
    public GitLabEventAction? Action { get; init; }

    /// <summary>Include only events against this kind of target.</summary>
    public GitLabEventTargetType? TargetType { get; init; }

    /// <summary>Include only events created before this date. A date, not a timestamp - GitLab compares by day.</summary>
    public DateOnly? Before { get; init; }

    /// <summary>Include only events created after this date. A date, not a timestamp - GitLab compares by day.</summary>
    public DateOnly? After { get; init; }

    /// <summary>Order by creation date; GitLab defaults to <see cref="GitLabEventSort.Desc" />.</summary>
    public GitLabEventSort? Sort { get; init; }

    public int? PerPage { get; init; }
}