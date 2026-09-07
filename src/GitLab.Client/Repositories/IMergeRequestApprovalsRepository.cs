using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Merge request approvals resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMergeRequestApprovalsService), typeof(IMergeRequestApprovalsClient))]
internal interface IMergeRequestApprovalsRepository
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