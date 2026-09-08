using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for project container repository protection rules, sitting between
///     the public <c>IProjectContainerRegistryProtectionRulesClient</c> controller and
///     <c>IProjectContainerRegistryProtectionRulesRepository</c>'s raw GitLab access. Mirrors the
///     repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface IProjectContainerRegistryProtectionRulesService
{
    IAsyncEnumerable<GitLabContainerRegistryProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}