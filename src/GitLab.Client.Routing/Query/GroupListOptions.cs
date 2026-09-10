using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Optional filters for <c>GET /groups</c>.</summary>
/// <remarks>
///     <c>page</c> is intentionally absent: <c>ListAsync</c> streams every page by following GitLab's
///     <c>Link</c> header. <c>custom_attributes</c> is also intentionally absent because GitLab declares it
///     as an unstructured object; exposing a dictionary here would invent an encoding contract that the
///     official specification does not define. Use <see cref="WithCustomAttributes" /> to include the
///     response projection.
/// </remarks>
[GitLabQuery]
public readonly record struct GroupListOptions
{
    public string? Search { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Group ids to exclude from the result.</summary>
    public IReadOnlyList<long>? SkipGroups { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Includes storage statistics. This filter is available to administrators only.</summary>
    public bool? Statistics { get; init; }

    /// <summary>Limits results to archived (<see langword="true" />) or non-archived groups.</summary>
    public bool? Archived { get; init; }

    /// <summary>Also includes groups visible to the caller where they are not a member.</summary>
    public bool? AllAvailable { get; init; }

    /// <summary>Limits results to groups owned by the authenticated user.</summary>
    public bool? Owned { get; init; }

    /// <summary>
    ///     Orders results by <c>name</c>, <c>path</c>, <c>id</c>, or <c>similarity</c> when searching.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Sort direction: <c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    /// <summary>Requires at least this numeric GitLab access level (10 Guest through 50 Owner).</summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>Returns only top-level groups.</summary>
    public bool? TopLevelOnly { get; init; }

    /// <summary>Returns only groups scheduled for deletion on this calendar date.</summary>
    public DateOnly? MarkedForDeletionOn { get; init; }

    /// <summary>Returns only groups that are neither archived nor scheduled for deletion.</summary>
    public bool? Active { get; init; }

    /// <summary>Limits results to a Gitaly repository storage shard. This filter is administrator-only.</summary>
    public string? RepositoryStorage { get; init; }

    /// <summary>Includes custom attributes in each response projection. This filter is administrator-only.</summary>
    public bool? WithCustomAttributes { get; init; }
}