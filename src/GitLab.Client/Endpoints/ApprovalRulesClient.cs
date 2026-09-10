using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ApprovalRulesClient(IGitLabApiConnection connection) : IApprovalRulesClient
{
    public IAsyncEnumerable<GitLabApprovalRule> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_rules").Build(),
            GitLabJsonContext.Default.GitLabApprovalRuleArray,
            cancellationToken);
    }

    public Task<GitLabApprovalRule> GetForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_rules")
                .Segment(approvalRuleId).Build(),
            GitLabJsonContext.Default.GitLabApprovalRule,
            cancellationToken);
    }

    public Task<GitLabApprovalRule> CreateForProjectAsync(ProjectId projectId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_rules").Build(),
            request,
            GitLabJsonContext.Default.CreateApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabApprovalRule,
            cancellationToken);
    }

    public Task<GitLabApprovalRule> UpdateForProjectAsync(ProjectId projectId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_rules")
                .Segment(approvalRuleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabApprovalRule,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_rules")
                .Segment(approvalRuleId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabApprovalRule> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("approval_rules").Build(),
            GitLabJsonContext.Default.GitLabApprovalRuleArray,
            cancellationToken);
    }

    public Task<GitLabApprovalRule> CreateForGroupAsync(GroupId groupId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return CreateGroupAsync(groupId,
            new CreateGroupApprovalRuleRequest
            {
                Name = request.Name,
                ApprovalsRequired = request.ApprovalsRequired,
                RuleType = request.RuleType,
                UserIds = request.UserIds,
                GroupIds = request.GroupIds
            }, cancellationToken);
    }

    public Task<GitLabApprovalRule> CreateGroupAsync(GroupId groupId, CreateGroupApprovalRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("approval_rules").Build(),
            request,
            GitLabJsonContext.Default.CreateGroupApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabApprovalRule,
            cancellationToken);
    }

    public Task<GitLabApprovalRule> UpdateForGroupAsync(GroupId groupId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return UpdateGroupAsync(groupId, approvalRuleId,
            new UpdateGroupApprovalRuleRequest
            {
                Name = request.Name,
                ApprovalsRequired = request.ApprovalsRequired,
                UserIds = request.UserIds,
                GroupIds = request.GroupIds
            }, cancellationToken);
    }

    public Task<GitLabApprovalRule> UpdateGroupAsync(GroupId groupId, long approvalRuleId,
        UpdateGroupApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("approval_rules").Segment(approvalRuleId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabApprovalRule,
            cancellationToken);
    }

    // The whole /approval_settings family answers with a single object rather than a paged collection -
    // the rules arrive nested inside it - so this is GetAsync, not GetPagedAsync.
    public Task<GitLabProjectApprovalSettings> GetSettingsForProjectAsync(ProjectId projectId,
        string? targetBranch = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_settings")
                .Query("target_branch", targetBranch).Build(),
            GitLabJsonContext.Default.GitLabProjectApprovalSettings,
            cancellationToken);
    }

    public Task<GitLabProjectApprovalSettingRule> CreateSettingsRuleForProjectAsync(ProjectId projectId,
        CreateApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SettingsRules(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabProjectApprovalSettingRule,
            cancellationToken);
    }

    public Task<GitLabProjectApprovalSettingRule> UpdateSettingsRuleForProjectAsync(ProjectId projectId,
        long approvalRuleId, UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            SettingsRules(projectId).Segment(approvalRuleId).Build(),
            request,
            GitLabJsonContext.Default.UpdateApprovalRuleRequest,
            GitLabJsonContext.Default.GitLabProjectApprovalSettingRule,
            cancellationToken);
    }

    public Task DeleteSettingsRuleForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            SettingsRules(projectId).Segment(approvalRuleId).Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder SettingsRules(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("approval_settings")
            .Literal("rules");
    }
}