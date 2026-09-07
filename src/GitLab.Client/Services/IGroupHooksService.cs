using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for GroupHooks, sitting between the public <c>IGroupHooksClient</c>
///     controller and <c>IGroupHooksRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IGroupHooksService
{
    IAsyncEnumerable<GitLabGroupHook> ListAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroupHook> GetAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default);

    Task<GitLabGroupHook> CreateAsync(GroupId groupId, CreateGroupHookRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupHook> UpdateAsync(GroupId groupId, long hookId, UpdateGroupHookRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default);

    Task TestAsync(GroupId groupId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(GroupId groupId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task ResendEventAsync(GroupId groupId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default);

    Task DeleteUrlVariableAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default);

    Task DeleteCustomHeaderAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default);
}