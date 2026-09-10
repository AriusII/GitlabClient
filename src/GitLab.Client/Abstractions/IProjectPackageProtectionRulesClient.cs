using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab project package protection rules API area
///     (<c>/projects/:id/packages/protection/rules</c>) - which packages, by name pattern and package
///     format, only members at or above a given role may push or delete.
/// </summary>
public interface IProjectPackageProtectionRulesClient
{
    /// <summary>Streams every package protection rule on the project.</summary>
    IAsyncEnumerable<GitLabPackageProtectionRule> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a package protection rule.</summary>
    Task<GitLabPackageProtectionRule> CreateAsync(ProjectId projectId, CreatePackageProtectionRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a package protection rule. Only the members set on the request are sent; anything left
    ///     null keeps its current value.
    /// </summary>
    Task<GitLabPackageProtectionRule> UpdateAsync(ProjectId projectId, long ruleId,
        UpdatePackageProtectionRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a package protection rule.</summary>
    Task DeleteAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}