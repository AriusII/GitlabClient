using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Paging for every repository-storage-move list endpoint - the three instance-wide feeds
///     (<c>/project_repository_storage_moves</c>, <c>/group_repository_storage_moves</c>,
///     <c>/snippet_repository_storage_moves</c>) and the three entity-scoped ones. <c>page</c> and
///     <c>per_page</c> are the only query parameters any of them declares, so one record serves all six.
/// </summary>
/// <remarks>
///     <c>page</c> is deliberately not exposed: the list methods stream every page by following GitLab's
///     RFC 5988 <c>Link: rel="next"</c> header, so pinning a start page would only skip results.
///     <see cref="PerPage" /> is the useful knob - draining a storage shard produces one move per
///     repository, so an instance-wide feed can run to tens of thousands of entries and the default page
///     size of 20 turns that into a great many round trips.
/// </remarks>
[GitLabQuery]
public sealed record StorageMoveListOptions
{
    /// <summary>Items per page. GitLab defaults to 20 and caps the value at 100.</summary>
    public int? PerPage { get; init; }
}