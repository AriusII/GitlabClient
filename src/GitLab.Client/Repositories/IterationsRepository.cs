using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class IterationsRepository(IGitLabApiConnection connection) : IIterationsRepository
{
    public IAsyncEnumerable<GitLabIteration> ListForGroupAsync(GroupId groupId, IterationListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("iterations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIterationArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIteration> ListForProjectAsync(ProjectId projectId,
        IterationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("iterations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIterationArray,
            cancellationToken);
    }
}