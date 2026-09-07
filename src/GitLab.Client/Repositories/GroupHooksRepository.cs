using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class GroupHooksRepository(IGitLabApiConnection connection) : IGroupHooksRepository
{
    public IAsyncEnumerable<GitLabGroupHook> ListAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Build(),
            GitLabJsonContext.Default.GitLabGroupHookArray,
            cancellationToken);
    }

    public Task<GitLabGroupHook> GetAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId).Build(),
            GitLabJsonContext.Default.GitLabGroupHook,
            cancellationToken);
    }

    public Task<GitLabGroupHook> CreateAsync(GroupId groupId, CreateGroupHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Build(),
            request,
            GitLabJsonContext.Default.CreateGroupHookRequest,
            GitLabJsonContext.Default.GitLabGroupHook,
            cancellationToken);
    }

    public Task<GitLabGroupHook> UpdateAsync(GroupId groupId, long hookId, UpdateGroupHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId).Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupHookRequest,
            GitLabJsonContext.Default.GitLabGroupHook,
            cancellationToken);
    }

    public Task DeleteAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default)
    {
        // GitLab's spec advertises 200 with the deleted hook here rather than the usual 204; the no-content
        // path is tolerant of both, so the body is simply not read.
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId).Build(),
            cancellationToken);
    }

    public Task TestAsync(GroupId groupId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        // Literal, not Escaped: the segment comes from a closed enum, never from caller-supplied text.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId)
                .Literal("test").Literal(trigger.ToRouteValue()).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(GroupId groupId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId)
                .Literal("events").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabHookEventArray,
            cancellationToken);
    }

    public Task ResendEventAsync(GroupId groupId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId)
                .Literal("events").Segment(hookLogId).Literal("resend").Build(),
            cancellationToken);
    }

    public Task DeleteUrlVariableAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId)
                .Literal("url_variables").Escaped(key).Build(),
            cancellationToken);
    }

    public Task DeleteCustomHeaderAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("hooks").Segment(hookId)
                .Literal("custom_headers").Escaped(key).Build(),
            cancellationToken);
    }
}