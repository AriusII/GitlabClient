using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the merge train list endpoints (<c>GET /projects/:id/merge_trains</c> and
///     <c>GET /projects/:id/merge_trains/:target_branch</c>). Both take the identical query.
/// </summary>
[GitLabQuery]
public sealed record MergeTrainListOptions
{
    /// <summary>
    ///     <c>active</c> for cars still queued, <c>complete</c> for cars that have already merged. Omitted,
    ///     GitLab returns both.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary><c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}