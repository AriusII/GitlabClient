using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class EventsRepository(IGitLabApiConnection connection) : IEventsRepository
{
    private const string EventsSegment = "events";

    public IAsyncEnumerable<GitLabEvent> ListAsync(EventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(EventsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabEventArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEvent> ListForProjectAsync(ProjectId projectId, EventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(EventsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabEventArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEvent> ListForUserAsync(long userId, EventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(EventsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabEventArray,
            cancellationToken);
    }
}