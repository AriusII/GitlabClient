using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for project container registry protection tag rules, sitting between
///     the public <c>IProjectContainerRegistryProtectionTagRulesClient</c> controller and
///     <c>IProjectContainerRegistryProtectionTagRulesRepository</c>'s raw GitLab access. Mirrors the
///     repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface IProjectContainerRegistryProtectionTagRulesService
{
    IAsyncEnumerable<GitLabContainerRegistryProtectionTagRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionTagRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionTagRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}