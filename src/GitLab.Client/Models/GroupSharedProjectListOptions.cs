using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /groups/:id/projects/shared</c> - projects shared into the group rather than
///     owned by it. GitLab declares a narrower parameter set here than for the owned-projects listing,
///     which is why this is a separate type.
/// </summary>
[GitLabQuery]
public sealed record GroupSharedProjectListOptions
{
    /// <summary>Filters on archive state. Null returns both archived and active entries.</summary>
    public bool? Archived { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Free-text filter on the name and path.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Field to order by. Left as a string because GitLab keeps adding to the vocabulary and an unknown value is
    ///     answered with a 400, not a silent 200.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Order direction, <c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    /// <summary>Returns a trimmed project payload, which is markedly cheaper on large groups.</summary>
    public bool? Simple { get; init; }

    /// <summary>Keeps only projects the caller starred.</summary>
    public bool? Starred { get; init; }

    public bool? WithIssuesEnabled { get; init; }

    public bool? WithMergeRequestsEnabled { get; init; }

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