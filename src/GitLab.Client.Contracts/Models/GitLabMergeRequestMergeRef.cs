namespace GitLab.Client.Models;

/// <summary>
///     The result of asking GitLab to merge a merge request into its default merge ref
///     (<c>GET /projects/:id/merge_requests/:iid/merge_ref</c>): the sha of the commit written to
///     <c>refs/merge-requests/:iid/merge</c>.
/// </summary>
public sealed record GitLabMergeRequestMergeRef
{
    /// <summary>The merge commit's sha, when GitLab could produce one.</summary>
    public string? CommitId { get; init; }
}