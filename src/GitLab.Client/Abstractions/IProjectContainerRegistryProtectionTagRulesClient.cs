using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab project container registry protection tag rules API area
///     (<c>/projects/:id/registry/protection/tag/rules</c>) - which container image tags, by name
///     pattern, only members at or above a given role may push or delete. Introduced in GitLab 18.7.
/// </summary>
public interface IProjectContainerRegistryProtectionTagRulesClient
{
    /// <summary>Streams every container registry protection tag rule on the project.</summary>
    IAsyncEnumerable<GitLabContainerRegistryProtectionTagRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a container registry protection tag rule. Unlike the repository-level rules, both
    ///     access levels are required - there is no unrestricted default for tags.
    /// </summary>
    Task<GitLabContainerRegistryProtectionTagRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a container registry protection tag rule. Only the members set on the request are sent;
    ///     anything left null keeps its current value.
    /// </summary>
    Task<GitLabContainerRegistryProtectionTagRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionTagRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a container registry protection tag rule.</summary>
    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}