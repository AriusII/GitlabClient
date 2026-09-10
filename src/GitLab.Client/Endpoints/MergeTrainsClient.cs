using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class MergeTrainsClient(IGitLabApiConnection connection) : IMergeTrainsClient
{
    public IAsyncEnumerable<GitLabMergeTrainCar> ListAsync(ProjectId projectId,
        MergeTrainListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_trains")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeTrainCarArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeTrainCar> ListForTargetBranchAsync(ProjectId projectId, string targetBranch,
        MergeTrainListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_trains")
                .Escaped(targetBranch)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeTrainCarArray,
            cancellationToken);
    }

    public Task<GitLabMergeTrainCar> GetStatusAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_trains")
                .Literal("merge_requests")
                .Segment(mergeRequestIid)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeTrainCar,
            cancellationToken);
    }

    public Task<GitLabMergeTrainCar> AddMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        AddToMergeTrainRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_trains")
                .Literal("merge_requests")
                .Segment(mergeRequestIid)
                .Build(),
            request,
            GitLabJsonContext.Default.AddToMergeTrainRequest,
            GitLabJsonContext.Default.GitLabMergeTrainCar,
            cancellationToken);
    }
}