using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for project package protection rules, sitting between the public
///     <c>IProjectPackageProtectionRulesClient</c> controller and
///     <c>IProjectPackageProtectionRulesRepository</c>'s raw GitLab access. Mirrors the repository's
///     method shapes 1:1 today (its implementation is generated); this is the seam where request
///     validation, caching, or cross-resource composition would go once the resource needs more than
///     pass-through.
/// </summary>
internal interface IProjectPackageProtectionRulesService
{
    IAsyncEnumerable<GitLabPackageProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabPackageProtectionRule> CreateAsync(ProjectId projectId, CreatePackageProtectionRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPackageProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdatePackageProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}