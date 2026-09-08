using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ProjectHooks resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectHooksService), typeof(IProjectHooksClient))]
internal interface IProjectHooksRepository
{
    IAsyncEnumerable<GitLabProjectHook> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProjectHook> GetAsync(ProjectId projectId, long hookId, CancellationToken cancellationToken = default);

    Task<GitLabProjectHook> AddAsync(ProjectId projectId, CreateProjectHookRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectHook> UpdateAsync(ProjectId projectId, long hookId, UpdateProjectHookRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long hookId, CancellationToken cancellationToken = default);

    Task TestAsync(ProjectId projectId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(ProjectId projectId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task ResendEventAsync(ProjectId projectId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default);

    Task DeleteUrlVariableAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default);

    Task DeleteCustomHeaderAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default);

    Task UpdateUrlVariableAsync(ProjectId projectId, long hookId, string key,
        UpdateProjectHookUrlVariableRequest request, CancellationToken cancellationToken = default);

    Task UpdateCustomHeaderAsync(ProjectId projectId, long hookId, string key,
        UpdateProjectHookCustomHeaderRequest request, CancellationToken cancellationToken = default);
}