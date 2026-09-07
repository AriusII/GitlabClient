using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the merge requests a commit belongs to
///     (<c>GET /projects/:id/repository/commits/:sha/merge_requests</c>).
/// </summary>
[GitLabQuery]
public sealed record CommitMergeRequestListOptions
{
    /// <summary>
    ///     GitLab's merge request state filter - <c>opened</c>, <c>closed</c>, <c>locked</c>, <c>merged</c>
    ///     or <c>all</c>. The spec leaves the value open, so it stays a string here.
    /// </summary>
    public string? State { get; init; }

    public int? PerPage { get; init; }
}