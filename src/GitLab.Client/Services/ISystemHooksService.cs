using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for SystemHooks, sitting between the public <c>ISystemHooksClient</c>
///     controller and <c>ISystemHooksRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ISystemHooksService
{
    IAsyncEnumerable<GitLabSystemHook> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabSystemHook> GetAsync(long hookId, CancellationToken cancellationToken = default);

    Task<GitLabSystemHook> CreateAsync(CreateSystemHookRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSystemHook> UpdateAsync(long hookId, UpdateSystemHookRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long hookId, CancellationToken cancellationToken = default);

    Task TestAsync(long hookId, CancellationToken cancellationToken = default);

    Task DeleteUrlVariableAsync(long hookId, string key, CancellationToken cancellationToken = default);

    Task DeleteCustomHeaderAsync(long hookId, string key, CancellationToken cancellationToken = default);

    Task UpdateUrlVariableAsync(long hookId, string key, UpdateSystemHookUrlVariableRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateCustomHeaderAsync(long hookId, string key, UpdateSystemHookCustomHeaderRequest request,
        CancellationToken cancellationToken = default);
}