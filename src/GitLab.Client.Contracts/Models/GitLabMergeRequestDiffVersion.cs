namespace GitLab.Client.Models;

/// <summary>
///     One version of a merge request's diff - the snapshot GitLab takes every time the source branch is
///     pushed to (<c>GET /projects/:id/merge_requests/:iid/versions</c>).
/// </summary>
/// <remarks>
///     <see cref="Commits" /> and <see cref="Diffs" /> are populated only by the single-version endpoint
///     (<c>versions/:version_id</c>); the listing omits both.
/// </remarks>
public sealed record GitLabMergeRequestDiffVersion
{
    public required long Id { get; init; }

    /// <summary>The commit the source branch pointed at when this version was taken.</summary>
    public string? HeadCommitSha { get; init; }

    /// <summary>The merge base between source and target at the time of this version.</summary>
    public string? BaseCommitSha { get; init; }

    /// <summary>The target branch's tip at the time of this version.</summary>
    public string? StartCommitSha { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The database id of the owning merge request - not its project-scoped iid.</summary>
    public long? MergeRequestId { get; init; }

    /// <summary>"collected", "overflow", or one of GitLab's other diff-collection states.</summary>
    public string? State { get; init; }

    /// <summary>
    ///     The number of changed files, as text. GitLab sends a string here because it appends a "+" once
    ///     the diff overflows its size limit.
    /// </summary>
    public string? RealSize { get; init; }

    public string? PatchIdSha { get; init; }

    /// <summary>The commits in this version. Only returned by the single-version endpoint.</summary>
    public IReadOnlyList<GitLabCommit>? Commits { get; init; }

    /// <summary>The per-file diffs in this version. Only returned by the single-version endpoint.</summary>
    public IReadOnlyList<GitLabDiff>? Diffs { get; init; }
}