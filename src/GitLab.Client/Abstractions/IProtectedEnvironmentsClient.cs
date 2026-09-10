using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Protected environments" API area
///     (<c>/projects/:id/protected_environments</c> and <c>/groups/:id/protected_environments</c>) - who
///     may deploy to an environment, and who must approve the deployment.
///     <para>
///         The two halves share a route shape but not a meaning: a project protects an environment by
///         <c>name</c>, a group protects a whole deployment tier, which is why the group methods take a
///         <c>deploymentTier</c>.
///     </para>
///     <para>
///         The project endpoints require CI/CD to be enabled on the project. With CI/CD disabled GitLab
///         answers <c>404</c> rather than <c>403</c>, so a <see cref="Exceptions.GitLabNotFoundException" />
///         here does not necessarily mean the environment is unprotected.
///     </para>
/// </summary>
public interface IProtectedEnvironmentsClient
{
    /// <summary>Streams every protected environment on a project.</summary>
    IAsyncEnumerable<GitLabProtectedEnvironment> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one protected environment by name. Environment names legally contain <c>/</c> and <c>*</c>
    ///     (<c>review/feature-x</c>); the route builder percent-encodes the name, so pass it raw.
    /// </summary>
    Task<GitLabProtectedEnvironment> GetForProjectAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>Protects a project environment, naming who may deploy to it and who must approve.</summary>
    Task<GitLabProtectedEnvironment> ProtectForProjectAsync(ProjectId projectId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the deploy-access entries and approval rules of an already protected project environment.</summary>
    Task<GitLabProtectedEnvironment> UpdateForProjectAsync(ProjectId projectId, string name,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Unprotects a project environment, leaving the environment itself in place.</summary>
    Task UnprotectForProjectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>Streams every protected deployment tier on a group.</summary>
    IAsyncEnumerable<GitLabProtectedEnvironment> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one protected deployment tier - <c>production</c>, <c>staging</c>, <c>testing</c>,
    ///     <c>development</c> or <c>other</c>.
    /// </summary>
    Task<GitLabProtectedEnvironment> GetForGroupAsync(GroupId groupId, string deploymentTier,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Protects a group deployment tier. <see cref="ProtectEnvironmentRequest.Name" /> carries the tier
    ///     here, not an environment name.
    /// </summary>
    Task<GitLabProtectedEnvironment> ProtectForGroupAsync(GroupId groupId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the deploy-access entries and approval rules of an already protected group deployment tier.</summary>
    Task<GitLabProtectedEnvironment> UpdateForGroupAsync(GroupId groupId, string deploymentTier,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Unprotects a group deployment tier.</summary>
    Task UnprotectForGroupAsync(GroupId groupId, string deploymentTier, CancellationToken cancellationToken = default);
}