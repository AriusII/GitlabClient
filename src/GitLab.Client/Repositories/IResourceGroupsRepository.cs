using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CI resource groups resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IResourceGroupsService), typeof(IResourceGroupsClient))]
internal interface IResourceGroupsRepository
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