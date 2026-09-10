namespace GitLab.Client.Models;

/// <summary>
///     A project's merge-request approval configuration (<c>/projects/:id/approvals</c>) - the project-wide
///     switches that govern approvals, as opposed to the individual rules in
///     <see cref="GitLabProjectApprovalSettings" />.
/// </summary>
/// <remarks>
///     Two of the booleans read backwards from their names, and GitLab's own parameter documentation says so:
///     <see cref="MergeRequestsAuthorApproval" /> <c>true</c> means authors <b>may</b> self-approve, while
///     <see cref="MergeRequestsDisableCommittersApproval" /> <c>true</c> means committers may <b>not</b>.
///     <see cref="GitLabMergeRequestApprovalSetting" /> is the newer, uniformly permissive spelling of the
///     same switches.
/// </remarks>
public sealed record GitLabProjectApprovalConfiguration
{
    /// <summary>
    ///     The individually named approvers. GitLab's OpenAPI document declares this member as a bare
    ///     reference rather than an array, which is a Grape artifact - the wire value is an array.
    /// </summary>
    public IReadOnlyList<GitLabApprover>? Approvers { get; init; }

    /// <summary>
    ///     The named approver groups. Declared as a bare reference for the same Grape reason as
    ///     <see cref="Approvers" />.
    /// </summary>
    public IReadOnlyList<GitLabApproverGroup>? ApproverGroups { get; init; }

    /// <summary>How many approvals a merge request needs before it can be merged.</summary>
    public int? ApprovalsBeforeMerge { get; init; }

    /// <summary>Whether a new push clears the approvals already given.</summary>
    public bool? ResetApprovalsOnPush { get; init; }

    /// <summary>Whether a push only clears the approvals of Code Owners whose files it touched.</summary>
    public bool? SelectiveCodeOwnerRemovals { get; init; }

    /// <summary>Whether individual merge requests are prevented from overriding the approvers and approval count.</summary>
    public bool? DisableOverridingApproversPerMergeRequest { get; init; }

    /// <summary>Whether merge request authors may approve their own merge requests.</summary>
    public bool? MergeRequestsAuthorApproval { get; init; }

    /// <summary>Whether committers are barred from approving.</summary>
    public bool? MergeRequestsDisableCommittersApproval { get; init; }

    /// <summary>Whether approvers must re-enter their password before approving.</summary>
    public bool? RequirePasswordToApprove { get; init; }

    /// <summary>Whether approvers must re-authenticate (password or SAML) before approving.</summary>
    public bool? RequireReauthenticationToApprove { get; init; }
}