using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "To-dos" API area (<c>/todos</c>) - the authenticated user's inbox of things
///     needing attention, plus the two endpoints that create an item on an issue or merge request.
///     <para>
///         Everything here is scoped to the token, not to a project: <c>GET /todos</c> has no project or
///         group path segment at all.
///     </para>
/// </summary>
public interface ITodosClient
{
    /// <summary>
    ///     Streams the authenticated user's to-do items (<c>GET /todos</c>), following pagination. With no
    ///     filters GitLab returns the pending items only.
    /// </summary>
    IAsyncEnumerable<GitLabTodo> ListAsync(TodoListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks a single to-do item as done (<c>POST /todos/:id/mark_as_done</c>) and returns the updated
    ///     item.
    /// </summary>
    Task<GitLabTodo> MarkAsDoneAsync(long todoId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks every pending to-do item as done (<c>POST /todos/mark_as_done</c>). GitLab answers with an
    ///     empty body, so there is nothing to return.
    /// </summary>
    Task MarkAllAsDoneAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a to-do item for the authenticated user on an issue
    ///     (<c>POST /projects/:id/issues/:issue_iid/todo</c> - note the singular segment). GitLab answers
    ///     <c>304 Not Modified</c> when an item already exists, which surfaces as a
    ///     <c>GitLabApiException</c> carrying <see cref="System.Net.HttpStatusCode.NotModified" /> rather
    ///     than as a no-op.
    /// </summary>
    Task<GitLabTodo> CreateForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a to-do item for the authenticated user on a merge request
    ///     (<c>POST /projects/:id/merge_requests/:merge_request_iid/todo</c> - note the singular segment).
    ///     GitLab answers <c>304 Not Modified</c> when an item already exists, which surfaces as a
    ///     <c>GitLabApiException</c> carrying <see cref="System.Net.HttpStatusCode.NotModified" /> rather
    ///     than as a no-op.
    /// </summary>
    Task<GitLabTodo> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);
}