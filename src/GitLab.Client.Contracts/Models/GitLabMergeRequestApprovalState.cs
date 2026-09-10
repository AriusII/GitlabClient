namespace GitLab.Client.Models;

/// <summary>
///     The rule-by-rule approval state of a merge request
///     (<c>GET /projects/:id/merge_requests/:iid/approval_state</c>). Despite the similar route name this is a
///     different shape from <see cref="GitLabMergeRequestApprovals" />.
/// </summary>
public sealed record GitLabMergeRequestApprovalState
{
    /// <summary>Whether this merge request's rules diverge from the project-level rules they were copied from.</summary>
    public bool? ApprovalRulesOverwritten { get; init; }

    public IReadOnlyList<GitLabMergeRequestApprovalRule>? Rules { get; init; }
}