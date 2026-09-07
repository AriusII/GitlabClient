using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for To-dos, sitting between the public <c>ITodosClient</c>
///     controller and <c>ITodosRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ITodosService
{
    IAsyncEnumerable<GitLabTodo> ListAsync(TodoListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTodo> MarkAsDoneAsync(long todoId, CancellationToken cancellationToken = default);

    Task MarkAllAsDoneAsync(CancellationToken cancellationToken = default);

    Task<GitLabTodo> CreateForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabTodo> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);
}