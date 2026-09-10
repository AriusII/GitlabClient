namespace GitLab.Client.Models;

/// <summary>
///     The merge-request approval settings of a project or a group
///     (<c>/projects/:id/merge_request_approval_setting</c>,
///     <c>/groups/:id/merge_request_approval_setting</c>).
/// </summary>
/// <remarks>
///     This is the uniformly permissive counterpart to <see cref="GitLabProjectApprovalConfiguration" />:
///     <c>allow_author_approval</c> says the same thing as that shape's
///     <c>merge_requests_author_approval</c>, and <c>retain_approvals_on_push</c> is the inverse of its
///     <c>reset_approvals_on_push</c>. Only this shape exists at group scope.
/// </remarks>
public sealed record GitLabMergeRequestApprovalSetting
{
    /// <summary>Whether merge request authors may approve their own merge requests.</summary>
    public bool? AllowAuthorApproval { get; init; }

    /// <summary>Whether committers may approve.</summary>
    public bool? AllowCommitterApproval { get; init; }

    /// <summary>Whether an individual merge request may override the inherited approver list.</summary>
    public bool? AllowOverridesToApproverListPerMergeRequest { get; init; }

    /// <summary>Whether approvals survive a new push.</summary>
    public bool? RetainApprovalsOnPush { get; init; }

    /// <summary>Whether a push only clears the approvals of Code Owners whose files it touched.</summary>
    public bool? SelectiveCodeOwnerRemovals { get; init; }

    /// <summary>Whether approvers must re-enter their password before approving.</summary>
    public bool? RequirePasswordToApprove { get; init; }

    /// <summary>Whether approvers must re-authenticate (password or SAML) before approving.</summary>
    public bool? RequireReauthenticationToApprove { get; init; }
}