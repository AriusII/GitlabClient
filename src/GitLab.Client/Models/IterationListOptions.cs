using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing iterations (<c>GET /groups/:id/iterations</c> and
///     <c>GET /projects/:id/iterations</c>; both endpoints take the identical parameter set).
/// </summary>
[GitLabQuery]
public sealed record IterationListOptions
{
    /// <summary>
    ///     Which iterations to return. A <b>string</b> on the wire, unlike the integer
    ///     <see cref="GitLabIteration.State" /> on the entity; GitLab's deprecated "started" value is not
    ///     exposed - see <see cref="GitLabIterationStateFilter" />.
    /// </summary>
    public GitLabIterationStateFilter? State { get; init; }

    /// <summary>Search criteria matched against the iteration title.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     The fields <see cref="Search" /> runs its fuzzy match against - GitLab accepts "title" and
    ///     "cadence_title" - sent as the <c>in</c> parameter. Omitting it searches the title only.
    /// </summary>
    [QueryParameter("in")]
    public IReadOnlyList<string>? SearchIn { get; init; }

    public bool? IncludeAncestors { get; init; }

    public bool? IncludeDescendants { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public int? PerPage { get; init; }
}