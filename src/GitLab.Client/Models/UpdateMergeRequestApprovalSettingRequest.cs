namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/merge_request_approval_setting</c> and
///     <c>PUT /groups/:id/merge_request_approval_setting</c>. Every member is optional; unset members are
///     omitted from the payload rather than sent as null, so a partial body leaves the rest of the settings
///     untouched.
/// </summary>
public sealed record UpdateMergeRequestApprovalSettingRequest
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