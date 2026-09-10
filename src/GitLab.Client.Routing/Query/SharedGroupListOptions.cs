using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for <c>GET /groups/:id/groups/shared</c> - the groups this group has been shared with.
/// </summary>
[GitLabQuery]
public readonly record struct SharedGroupListOptions
{
    /// <summary>Group IDs to leave out of the result.</summary>
    public IReadOnlyList<long>? SkipGroups { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Free-text filter on the name and path.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Keeps only entries where the caller holds at least this role, on GitLab's numeric ladder (10 Guest ... 50
    ///     Owner).
    /// </summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>
    ///     Field to order by. Left as a string because GitLab keeps adding to the vocabulary and an unknown value is
    ///     answered with a 400, not a silent 200.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Order direction, <c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    /// <summary>Includes each entry's custom attributes. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}