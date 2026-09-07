namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>PUT /projects/:id/merge_requests/:merge_request_iid/merge</c>. Every property is
///     optional - merging with all of them left null is the plain "merge it now" call.
/// </summary>
public sealed record MergeMergeRequestRequest
{
    /// <summary>Overrides the merge commit message GitLab would compose.</summary>
    public string? MergeCommitMessage { get; init; }

    /// <summary>Overrides the squash commit message, when <see cref="Squash" /> is on.</summary>
    public string? SquashCommitMessage { get; init; }

    /// <summary>Deletes the source branch once the merge lands.</summary>
    public bool? ShouldRemoveSourceBranch { get; init; }

    /// <summary>
    ///     Schedules the merge for when the pipeline succeeds instead of merging now. GitLab's newer
    ///     spelling of this is <see cref="AutoMerge" />.
    /// </summary>
    public bool? MergeWhenPipelineSucceeds { get; init; }

    /// <summary>Schedules the merge under GitLab's auto-merge strategy.</summary>
    public bool? AutoMerge { get; init; }

    /// <summary>
    ///     The head the caller believes it is merging. GitLab refuses the merge if the source branch has
    ///     moved on - pass it to make the merge safe against a concurrent push.
    /// </summary>
    public string? Sha { get; init; }

    /// <summary>Squashes the source branch's commits into one.</summary>
    public bool? Squash { get; init; }

    /// <summary>Merges immediately rather than adding the merge request to the project's merge train.</summary>
    public bool? SkipMergeTrain { get; init; }
}