using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ProjectHooks, sitting between the public <c>IProjectHooksClient</c>
///     controller and <c>IProjectHooksRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IProjectHooksService
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
}