using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MergeRequestApprovalsRepository(IGitLabApiConnection connection)
    : IMergeRequestApprovalsRepository
{
    public Task<GitLabMergeRequestApprovals> GetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approvals").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestApprovals,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovals> ApproveAsync(ProjectId projectId, long mergeRequestIid,
        ApproveMergeRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        Uri route = MergeRequestRoute(projectId, mergeRequestIid).Literal("approve").Build();

        // Every member of the body is optional, so a caller with nothing to say sends no body at all rather
        // than an empty JSON object.
        return request is null
            ? connection.PostAsync(
                route,
                GitLabJsonContext.Default.GitLabMergeRequestApprovals,
                cancellationToken)
            : connection.PostAsync(
                route,
                request,
                GitLabJsonContext.Default.ApproveMergeRequestRequest,
                GitLabJsonContext.Default.GitLabMergeRequestApprovals,
                cancellationToken);
    }

    public Task<GitLabMergeRequestApprovals> UnapproveAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("unapprove").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestApprovals,
            cancellationToken);
    }

    public Task ResetApprovalsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        // A body-less PUT answered with 200 and no content - hence the no-content PutAsync overload.
        return connection.PutAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("reset_approvals").Build(),
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalState> GetApprovalStateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_state").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestApprovalState,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestApprovalRule> ListRulesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_rules").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestApprovalRuleArray,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalRule> GetRuleAsync(ProjectId projectId, long mergeRequestIid,
        long approvalRuleId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_rules").Segment(approvalRuleId).Build(),
            GitLabJsonContext.Default.GitLabMergeRequestApprovalRule,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalRule> CreateRuleAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_rules").Build(),
            request,
            GitLabJsonContext.Default.CreateMergeRequestApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabMergeRequestApprovalRule,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalRule> UpdateRuleAsync(ProjectId projectId, long mergeRequestIid,
        long approvalRuleId, UpdateMergeRequestApprovalRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_rules").Segment(approvalRuleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateMergeRequestApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabMergeRequestApprovalRule,
            cancellationToken);
    }

    public Task DeleteRuleAsync(ProjectId projectId, long mergeRequestIid, long approvalRuleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("approval_rules").Segment(approvalRuleId).Build(),
            cancellationToken);
    }

    public Task<GitLabProjectApprovalConfiguration> GetApprovalConfigurationAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approvals").Build(),
            GitLabJsonContext.Default.GitLabProjectApprovalConfiguration,
            cancellationToken);
    }

    // Create-or-update in one verb: GitLab answers this POST with 201 whether or not a configuration
    // already existed, so there is no separate update route to model.
    public Task<GitLabProjectApprovalConfiguration> SetApprovalConfigurationAsync(ProjectId projectId,
        SetApprovalConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approvals").Build(),
            request,
            GitLabJsonContext.Default.SetApprovalConfigurationRequest,
            GitLabJsonContext.Default.GitLabProjectApprovalConfiguration,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalSetting> GetSettingsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectApprovalSetting(projectId),
            GitLabJsonContext.Default.GitLabMergeRequestApprovalSetting,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForProjectAsync(ProjectId projectId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectApprovalSetting(projectId),
            request,
            GitLabJsonContext.Default.UpdateMergeRequestApprovalSettingRequest,
            GitLabJsonContext.Default.GitLabMergeRequestApprovalSetting,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalSetting> GetSettingsForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupApprovalSetting(groupId),
            GitLabJsonContext.Default.GitLabMergeRequestApprovalSetting,
            cancellationToken);
    }

    public Task<GitLabMergeRequestApprovalSetting> UpdateSettingsForGroupAsync(GroupId groupId,
        UpdateMergeRequestApprovalSettingRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupApprovalSetting(groupId),
            request,
            GitLabJsonContext.Default.UpdateMergeRequestApprovalSettingRequest,
            GitLabJsonContext.Default.GitLabMergeRequestApprovalSetting,
            cancellationToken);
    }

    // Singular "setting" on the wire, and deliberately not pluralised here - the route word is what GitLab
    // serves, not a typo to be tidied.
    private static Uri ProjectApprovalSetting(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_request_approval_setting")
            .Build();
    }

    private static Uri GroupApprovalSetting(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("merge_request_approval_setting")
            .Build();
    }

    /// <summary>
    ///     Every merge-request-scoped route in this resource hangs off the same merge request, addressed by its
    ///     <c>iid</c>. The builder is mutable and chained, so each call returns a fresh one rather than a
    ///     shared instance.
    /// </summary>
    private static GitLabRouteBuilder MergeRequestRoute(ProjectId projectId, long mergeRequestIid)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid);
    }
}