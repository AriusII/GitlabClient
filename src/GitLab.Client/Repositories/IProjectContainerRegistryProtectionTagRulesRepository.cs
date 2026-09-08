using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the project container registry protection tag rules resource: builds
///     routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectContainerRegistryProtectionTagRulesService),
    typeof(IProjectContainerRegistryProtectionTagRulesClient))]
internal interface IProjectContainerRegistryProtectionTagRulesRepository
{
    IAsyncEnumerable<GitLabContainerRegistryProtectionTagRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionTagRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    Task<GitLabContainerRegistryProtectionTagRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}