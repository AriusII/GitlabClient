using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the To-dos resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         The collection lives at the root <c>/todos</c> - it is scoped entirely to the authenticated
///         token, with no project or group segment - while the create endpoints hang off an issuable and
///         use the SINGULAR <c>todo</c> segment.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(ITodosService), typeof(ITodosClient))]
internal interface ITodosRepository
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