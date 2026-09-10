using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Paging control for the resource event feeds
///     (<c>/projects/:id/issues/:iid/resource_*_events</c>,
///     <c>/projects/:id/merge_requests/:iid/resource_*_events</c>,
///     <c>/groups/:id/epics/:iid/resource_*_events</c>). Every one of those endpoints accepts exactly
///     <c>page</c> and <c>per_page</c> and nothing else, so one options record serves all of them.
/// </summary>
/// <remarks>
///     <c>page</c> is deliberately not exposed: the list methods stream every page by following GitLab's
///     RFC 5988 <c>Link: rel="next"</c> header, so pinning a starting page would silently truncate nothing
///     and skip everything before it. <see cref="PerPage" /> is the useful knob - a busy issue can carry
///     hundreds of label events, and raising the page size cuts the number of round trips.
/// </remarks>
[GitLabQuery]
public readonly record struct ResourceEventListOptions
{
    /// <summary>Items per page. GitLab defaults to 20 and caps the value at 100.</summary>
    public int? PerPage { get; init; }
}