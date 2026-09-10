namespace GitLab.Client.Models;

/// <summary>
///     The approval summary for a merge request (<c>GET /projects/:id/merge_requests/:iid/approvals</c>, and the
///     response to approve/unapprove). Distinct from <see cref="GitLabMergeRequestApprovalState" />, which
///     describes the rules rather than who has approved.
/// </summary>
public sealed record GitLabMergeRequestApprovals
{
    /// <summary>Whether the authenticated user has already approved this merge request.</summary>
    public bool? UserHasApproved { get; init; }

    /// <summary>Whether the authenticated user is an eligible approver.</summary>
    public bool? UserCanApprove { get; init; }

    /// <summary>Whether the merge request's approval requirements are currently satisfied.</summary>
    public bool? Approved { get; init; }

    /// <summary>
    ///     Every approver of the merge request, regardless of whether their approval satisfies a rule. GitLab's
    ///     OpenAPI document declares this member as a bare <c>$ref</c> rather than an array, which is a Grape
    ///     artifact - the wire value is an array.
    /// </summary>
    public IReadOnlyList<GitLabApproval>? ApprovedBy { get; init; }
}