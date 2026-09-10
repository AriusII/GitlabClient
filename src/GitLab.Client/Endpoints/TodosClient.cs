using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class TodosClient(IGitLabApiConnection connection) : ITodosClient
{
    public IAsyncEnumerable<GitLabTodo> ListAsync(TodoListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("todos")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabTodoArray,
            cancellationToken);
    }

    public Task<GitLabTodo> MarkAsDoneAsync(long todoId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("todos")
                .Segment(todoId)
                .Literal("mark_as_done")
                .Build(),
            GitLabJsonContext.Default.GitLabTodo,
            cancellationToken);
    }

    public Task MarkAllAsDoneAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("todos")
                .Literal("mark_as_done")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabTodo> CreateForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("issues")
                .Segment(issueIid)
                .Literal("todo")
                .Build(),
            GitLabJsonContext.Default.GitLabTodo,
            cancellationToken);
    }

    public Task<GitLabTodo> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_requests")
                .Segment(mergeRequestIid)
                .Literal("todo")
                .Build(),
            GitLabJsonContext.Default.GitLabTodo,
            cancellationToken);
    }
}