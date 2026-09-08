using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the project package protection rules resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectPackageProtectionRulesService), typeof(IProjectPackageProtectionRulesClient))]
internal interface IProjectPackageProtectionRulesRepository
{
    IAsyncEnumerable<GitLabPackageProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabPackageProtectionRule> CreateAsync(ProjectId projectId, CreatePackageProtectionRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPackageProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdatePackageProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}