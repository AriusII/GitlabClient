namespace GitLab.Client.Models;

/// <summary>
///     GitLab's answer to a rebase request (<c>PUT /projects/:id/merge_requests/:iid/rebase</c>), which is
///     accepted asynchronously: a <c>202</c> only says the rebase was queued.
/// </summary>
/// <remarks>
///     Poll the merge request with <c>include_rebase_in_progress</c> to find out how it finished; a failed
///     rebase surfaces there as <c>merge_error</c>.
/// </remarks>
public sealed record GitLabMergeRequestRebaseResult
{
    /// <summary>True while GitLab is still running the rebase.</summary>
    public bool? RebaseInProgress { get; init; }

    /// <summary>Why the rebase could not be started, when it could not be.</summary>
    public string? MergeError { get; init; }
}