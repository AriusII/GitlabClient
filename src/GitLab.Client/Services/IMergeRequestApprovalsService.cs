using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Merge request approvals, sitting between the public
///     <c>IMergeRequestApprovalsClient</c> controller and <c>IMergeRequestApprovalsRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IMergeRequestApprovalsService
{
    Task<GitLabMergeRequestApprovals> GetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovals> ApproveAsync(ProjectId projectId, long mergeRequestIid,
        ApproveMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovals> UnapproveAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task ResetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalState> GetApprovalStateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestApprovalRule> ListRulesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalRule> GetRuleAsync(ProjectId projectId, long mergeRequestIid, long approvalRuleId,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalRule> CreateRuleAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestApprovalRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalRule> UpdateRuleAsync(ProjectId projectId, long mergeRequestIid,
        long approvalRuleId, UpdateMergeRequestApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteRuleAsync(ProjectId projectId, long mergeRequestIid, long approvalRuleId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectApprovalConfiguration> GetApprovalConfigurationAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectApprovalConfiguration> SetApprovalConfigurationAsync(ProjectId projectId,
        SetApprovalConfigurationRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalSetting> GetSettingsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForProjectAsync(ProjectId projectId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalSetting> GetSettingsForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForGroupAsync(GroupId groupId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default);
}