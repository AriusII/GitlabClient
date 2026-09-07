using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Approval rules, sitting between the public
///     <c>IApprovalRulesClient</c> controller and <c>IApprovalRulesRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more than
///     pass-through.
/// </summary>
internal interface IApprovalRulesService
{
    IAsyncEnumerable<GitLabApprovalRule> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabApprovalRule> GetForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);

    Task<GitLabApprovalRule> CreateForProjectAsync(ProjectId projectId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApprovalRule> UpdateForProjectAsync(ProjectId projectId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabApprovalRule> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabApprovalRule> CreateForGroupAsync(GroupId groupId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApprovalRule> UpdateForGroupAsync(GroupId groupId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabProjectApprovalSettings> GetSettingsForProjectAsync(ProjectId projectId, string? targetBranch = null,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectApprovalSettingRule> CreateSettingsRuleForProjectAsync(ProjectId projectId,
        CreateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabProjectApprovalSettingRule> UpdateSettingsRuleForProjectAsync(ProjectId projectId,
        long approvalRuleId, UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteSettingsRuleForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);
}