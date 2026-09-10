using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class EventsClient(IGitLabApiConnection connection) : IEventsClient
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

    public IAsyncEnumerable<GitLabEvent> ListForUserAsync(string userId, EventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users")
                .Escaped(userId)
                .Literal(EventsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabEventArray,
            cancellationToken);
    }
}