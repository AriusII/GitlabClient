using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Paging options shared by the issue and merge-request listings under a project or group milestone.
///     Those four GitLab routes expose only <c>page</c> and <c>per_page</c>; the client owns the starting page
///     and follows <c>Link: rel="next"</c>, leaving <see cref="PerPage" /> as the useful caller-controlled
///     throughput setting.
/// </summary>
[GitLabQuery]
public readonly record struct MilestoneIssuableListOptions
{
    /// <summary>The maximum number of issuables GitLab should return in each fetched page.</summary>
    public int? PerPage { get; init; }
}