using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters shared by the two group-hierarchy listings, <c>GET /groups/:id/subgroups</c> and
///     <c>GET /groups/:id/descendant_groups</c>. GitLab declares the same parameter set for both;
///     only the depth differs - subgroups are the direct children, descendants are the whole subtree.
/// </summary>
[GitLabQuery]
public sealed record GroupHierarchyListOptions
{
    /// <summary>Includes storage statistics. Administrators only.</summary>
    public bool? Statistics { get; init; }

    /// <summary>Filters on archive state. Null returns both archived and active entries.</summary>
    public bool? Archived { get; init; }

    /// <summary>Group IDs to leave out of the result.</summary>
    public IReadOnlyList<long>? SkipGroups { get; init; }

    /// <summary>Includes groups the caller is not a member of but can see.</summary>
    public bool? AllAvailable { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Free-text filter on the name and path.</summary>
    public string? Search { get; init; }

    /// <summary>Keeps only entries the caller owns.</summary>
    public bool? Owned { get; init; }

    /// <summary>
    ///     Field to order by. Left as a string because GitLab keeps adding to the vocabulary and an unknown value is
    ///     answered with a 400, not a silent 200.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Order direction, <c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    /// <summary>
    ///     Keeps only entries where the caller holds at least this role, on GitLab's numeric ladder (10 Guest ... 50
    ///     Owner).
    /// </summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>Keeps only top-level groups.</summary>
    public bool? TopLevelOnly { get; init; }

    /// <summary>Keeps only entries scheduled for deletion on this date.</summary>
    public DateOnly? MarkedForDeletionOn { get; init; }

    /// <summary>Filters on active state - the inverse of archived plus pending deletion.</summary>
    public bool? Active { get; init; }

    /// <summary>Keeps only entries on this storage shard. Administrators only.</summary>
    public string? RepositoryStorage { get; init; }

    /// <summary>Includes each entry's custom attributes. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}