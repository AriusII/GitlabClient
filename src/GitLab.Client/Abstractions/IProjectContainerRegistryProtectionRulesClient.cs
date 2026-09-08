using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab project container repository protection rules API area
///     (<c>/projects/:id/registry/protection/repository/rules</c>) - which container image repositories,
///     by path pattern, only members at or above a given role may push images to or delete images from.
/// </summary>
public interface IProjectContainerRegistryProtectionRulesClient
{
    /// <summary>Streams every container repository protection rule on the project.</summary>
    IAsyncEnumerable<GitLabContainerRegistryProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a container repository protection rule.</summary>
    Task<GitLabContainerRegistryProtectionRule> CreateAsync(ProjectId projectId,
        CreateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a container repository protection rule. Only the members set on the request are sent;
    ///     anything left null keeps its current value.
    /// </summary>
    Task<GitLabContainerRegistryProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdateContainerRegistryProtectionRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a container repository protection rule.</summary>
    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}