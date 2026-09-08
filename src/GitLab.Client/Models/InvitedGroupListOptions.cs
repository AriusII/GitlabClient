using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /groups/:id/invited_groups</c> - the groups invited into this one.
/// </summary>
[GitLabQuery]
public readonly record struct InvitedGroupListOptions
{
    /// <summary>Which invitations to include: <c>direct</c>, <c>inherited</c>, or both.</summary>
    public IReadOnlyList<string>? Relation { get; init; }

    /// <summary>Free-text filter on the name and path.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Keeps only entries where the caller holds at least this role, on GitLab's numeric ladder (10 Guest ... 50
    ///     Owner).
    /// </summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>Includes each entry's custom attributes. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}