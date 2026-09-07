namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/approvals</c>, which creates or updates a project's approval
///     configuration in one call. Every member is optional; unset members are omitted from the payload rather
///     than sent as null, so a partial body leaves the rest of the configuration untouched.
/// </summary>
/// <remarks>
///     See <see cref="GitLabProjectApprovalConfiguration" /> for the two members whose polarity is the
///     opposite of what their names suggest.
/// </remarks>
public sealed record SetApprovalConfigurationRequest
{
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