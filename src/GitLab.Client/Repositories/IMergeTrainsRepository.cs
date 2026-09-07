using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Merge trains resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMergeTrainsService), typeof(IMergeTrainsClient))]
internal interface IMergeTrainsRepository
{
    IAsyncEnumerable<GitLabMergeTrainCar> ListAsync(ProjectId projectId, MergeTrainListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeTrainCar> ListForTargetBranchAsync(ProjectId projectId, string targetBranch,
        MergeTrainListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabMergeTrainCar> GetStatusAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeTrainCar> AddMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        AddToMergeTrainRequest request, CancellationToken cancellationToken = default);
}