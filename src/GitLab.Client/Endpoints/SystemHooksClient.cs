using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class SystemHooksClient(IGitLabApiConnection connection) : ISystemHooksClient
{
    public IAsyncEnumerable<GitLabSystemHook> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("hooks").Build(),
            GitLabJsonContext.Default.GitLabSystemHookArray,
            cancellationToken);
    }

    public Task<GitLabSystemHook> GetAsync(long hookId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Build(),
            GitLabJsonContext.Default.GitLabSystemHook,
            cancellationToken);
    }

    public Task<GitLabSystemHook> CreateAsync(CreateSystemHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("hooks").Build(),
            request,
            GitLabJsonContext.Default.CreateSystemHookRequest,
            GitLabJsonContext.Default.GitLabSystemHook,
            cancellationToken);
    }

    public Task<GitLabSystemHook> UpdateAsync(long hookId, UpdateSystemHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Build(),
            request,
            GitLabJsonContext.Default.UpdateSystemHookRequest,
            GitLabJsonContext.Default.GitLabSystemHook,
            cancellationToken);
    }

    public Task DeleteAsync(long hookId, CancellationToken cancellationToken = default)
    {
        // GitLab's spec advertises 200 with the deleted hook here rather than the usual 204; the no-content
        // path is tolerant of both, so the body is simply not read.
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Build(),
            cancellationToken);
    }

    public Task TestAsync(long hookId, CancellationToken cancellationToken = default)
    {
        // POST to the hook's own URL is the system-hook test run - there is no /test/{trigger} form here.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Build(),
            cancellationToken);
    }

    public Task DeleteUrlVariableAsync(long hookId, string key, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Literal("url_variables").Escaped(key).Build(),
            cancellationToken);
    }

    public Task DeleteCustomHeaderAsync(long hookId, string key, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Literal("custom_headers").Escaped(key).Build(),
            cancellationToken);
    }

    public Task UpdateUrlVariableAsync(long hookId, string key, UpdateSystemHookUrlVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        // 200 with no body: the value being set is secret-adjacent, and GitLab never echoes it back.
        return connection.PutAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Literal("url_variables").Escaped(key).Build(),
            request,
            GitLabJsonContext.Default.UpdateSystemHookUrlVariableRequest,
            cancellationToken);
    }

    public Task UpdateCustomHeaderAsync(long hookId, string key, UpdateSystemHookCustomHeaderRequest request,
        CancellationToken cancellationToken = default)
    {
        // 200 with no body: the value being set is secret-adjacent, and GitLab never echoes it back.
        return connection.PutAsync(
            GitLabRouteBuilder.Create("hooks").Segment(hookId).Literal("custom_headers").Escaped(key).Build(),
            request,
            GitLabJsonContext.Default.UpdateSystemHookCustomHeaderRequest,
            cancellationToken);
    }
}