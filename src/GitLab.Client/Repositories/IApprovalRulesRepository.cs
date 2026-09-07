using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Approval rules resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IApprovalRulesService), typeof(IApprovalRulesClient))]
internal interface IApprovalRulesRepository
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