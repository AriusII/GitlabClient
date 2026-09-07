using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the GroupHooks resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IGroupHooksService), typeof(IGroupHooksClient))]
internal interface IGroupHooksRepository
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