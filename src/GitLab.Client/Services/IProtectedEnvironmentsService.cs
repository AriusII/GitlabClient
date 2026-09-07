using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Protected environments, sitting between the public
///     <c>IProtectedEnvironmentsClient</c> controller and <c>IProtectedEnvironmentsRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is generated);
///     this is the seam where request validation, caching, or cross-resource composition would go once
///     the resource needs more than pass-through.
/// </summary>
internal interface IProtectedEnvironmentsService
{
    IAsyncEnumerable<GitLabProtectedEnvironment> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> GetForProjectAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> ProtectForProjectAsync(ProjectId projectId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> UpdateForProjectAsync(ProjectId projectId, string name,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    Task UnprotectForProjectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProtectedEnvironment> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> GetForGroupAsync(GroupId groupId, string deploymentTier,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> ProtectForGroupAsync(GroupId groupId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> UpdateForGroupAsync(GroupId groupId, string deploymentTier,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    Task UnprotectForGroupAsync(GroupId groupId, string deploymentTier, CancellationToken cancellationToken = default);
}