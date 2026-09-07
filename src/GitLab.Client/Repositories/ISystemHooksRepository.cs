using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the SystemHooks resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISystemHooksService), typeof(ISystemHooksClient))]
internal interface ISystemHooksRepository
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