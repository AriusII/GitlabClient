using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the project container repository protection rules resource: builds
///     routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectContainerRegistryProtectionRulesService),
    typeof(IProjectContainerRegistryProtectionRulesClient))]
internal interface IProjectContainerRegistryProtectionRulesRepository
{
    IAsyncEnumerable<GitLabContainerRegistryProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}