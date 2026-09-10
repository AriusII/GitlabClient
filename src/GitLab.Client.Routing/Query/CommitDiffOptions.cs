using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Options for a commit's diff (<c>GET /projects/:id/repository/commits/:sha/diff</c>).</summary>
[GitLabQuery]
public readonly record struct CommitDiffOptions
{
    /// <summary>
    ///     Asks GitLab to render <see cref="GitLabDiff.Diff" /> in unified-diff format, headers included,
    ///     rather than the bare hunk text it returns by default.
    /// </summary>
    public bool? Unidiff { get; init; }

    public int? PerPage { get; init; }
}