using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Merge request approvals" API area
///     (<c>/projects/:id/merge_requests/:iid/approvals</c> and its sibling approval routes).
/// </summary>
/// <remarks>
///     Every operation here is keyed by the merge request's <c>iid</c> (its per-project number), never its
///     global <c>id</c>. Approval rules, by contrast, are addressed by their own <c>id</c>.
/// </remarks>
public interface IMergeRequestApprovalsClient
{
    /// <summary>
    ///     Retrieves who has approved a merge request and whether its approval requirements are met
    ///     (<c>GET /projects/:id/merge_requests/:iid/approvals</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovals> GetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves a merge request as the authenticated user
    ///     (<c>POST /projects/:id/merge_requests/:iid/approve</c>). The caller must be an eligible approver.
    /// </summary>
    /// <remarks>
    ///     The request body is optional: supply one to pin the approval to a specific HEAD SHA, publish pending
    ///     review comments, or satisfy a project that requires explicit authentication on approval. Omit it and
    ///     no body is sent at all.
    /// </remarks>
    Task<GitLabMergeRequestApprovals> ApproveAsync(ProjectId projectId, long mergeRequestIid,
        ApproveMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the authenticated user's approval from a merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/unapprove</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovals> UnapproveAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resets every approval on a merge request
    ///     (<c>PUT /projects/:id/merge_requests/:iid/reset_approvals</c>).
    /// </summary>
    /// <remarks>
    ///     GitLab restricts this endpoint to bot users authenticating with a project or group access token; a
    ///     human personal access token is answered with <c>401 Unauthorized</c>, surfaced here as a
    ///     <c>GitLabAuthenticationException</c>.
    /// </remarks>
    Task ResetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the rule-by-rule approval state of a merge request
    ///     (<c>GET /projects/:id/merge_requests/:iid/approval_state</c>). This is the rules view;
    ///     <see cref="GetApprovalsAsync" /> is the approvers view.
    /// </summary>
    Task<GitLabMergeRequestApprovalState> GetApprovalStateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge request's own approval rules
    ///     (<c>GET /projects/:id/merge_requests/:iid/approval_rules</c>), following pagination as it goes.
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestApprovalRule> ListRulesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one merge-request approval rule
    ///     (<c>GET /projects/:id/merge_requests/:iid/approval_rules/:approval_rule_id</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovalRule> GetRuleAsync(ProjectId projectId, long mergeRequestIid, long approvalRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an approval rule on a merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/approval_rules</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovalRule> CreateRuleAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestApprovalRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a merge-request approval rule
    ///     (<c>PUT /projects/:id/merge_requests/:iid/approval_rules/:approval_rule_id</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovalRule> UpdateRuleAsync(ProjectId projectId, long mergeRequestIid,
        long approvalRuleId, UpdateMergeRequestApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a merge-request approval rule
    ///     (<c>DELETE /projects/:id/merge_requests/:iid/approval_rules/:approval_rule_id</c>).
    /// </summary>
    Task DeleteRuleAsync(ProjectId projectId, long mergeRequestIid, long approvalRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the project-wide approval configuration (<c>GET /projects/:id/approvals</c>): the named
    ///     approvers and approver groups, plus the switches that govern how approvals behave.
    /// </summary>
    Task<GitLabProjectApprovalConfiguration> GetApprovalConfigurationAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or updates the project-wide approval configuration (<c>POST /projects/:id/approvals</c>).
    ///     Unset members of the request are omitted from the payload and stay unchanged.
    /// </summary>
    /// <remarks>The authenticated user must be an eligible approver on the project.</remarks>
    Task<GitLabProjectApprovalConfiguration> SetApprovalConfigurationAsync(ProjectId projectId,
        SetApprovalConfigurationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a project's merge-request approval settings
    ///     (<c>GET /projects/:id/merge_request_approval_setting</c>). This is the permissively named view of
    ///     the same switches <see cref="GetApprovalConfigurationAsync" /> returns - see
    ///     <see cref="GitLabMergeRequestApprovalSetting" /> for how the two line up.
    /// </summary>
    Task<GitLabMergeRequestApprovalSetting> GetSettingsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project's merge-request approval settings
    ///     (<c>PUT /projects/:id/merge_request_approval_setting</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForProjectAsync(ProjectId projectId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a group's merge-request approval settings
    ///     (<c>GET /groups/:id/merge_request_approval_setting</c>), which every project below it inherits.
    /// </summary>
    Task<GitLabMergeRequestApprovalSetting> GetSettingsForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a group's merge-request approval settings
    ///     (<c>PUT /groups/:id/merge_request_approval_setting</c>).
    /// </summary>
    Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForGroupAsync(GroupId groupId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default);
}