namespace GitLab.Client.Models;

/// <summary>
///     The result of comparing two refs (<c>GET /projects/:id/repository/compare</c>).
/// </summary>
public sealed record GitLabCompare
{
    /// <summary>The most recent commit in the comparison range.</summary>
    public GitLabCommit? Commit { get; init; }

    public IReadOnlyList<GitLabCommit>? Commits { get; init; }

    public IReadOnlyList<GitLabDiff>? Diffs { get; init; }

    /// <summary>True when GitLab gave up building the diff before it was complete.</summary>
    public bool? CompareTimeout { get; init; }

    /// <summary>True when both sides of the comparison resolve to the same ref.</summary>
    public bool? CompareSameRef { get; init; }

    public Uri? WebUrl { get; init; }
}