using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI resource groups, sitting between the public
///     <c>IResourceGroupsClient</c> controller and <c>IResourceGroupsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IResourceGroupsService
{
    IAsyncEnumerable<GitLabResourceGroup> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabResourceGroup> GetAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabResourceGroup> UpdateAsync(ProjectId projectId, string key, UpdateResourceGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabJob> GetCurrentJobAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabJob> ListUpcomingJobsAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);
}