using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ProjectHooksRepository(IGitLabApiConnection connection) : IProjectHooksRepository
{
    public IAsyncEnumerable<GitLabProjectHook> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Build(),
            GitLabJsonContext.Default.GitLabProjectHookArray,
            cancellationToken);
    }

    public Task<GitLabProjectHook> GetAsync(ProjectId projectId, long hookId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId).Build(),
            GitLabJsonContext.Default.GitLabProjectHook,
            cancellationToken);
    }

    public Task<GitLabProjectHook> AddAsync(ProjectId projectId, CreateProjectHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Build(),
            request,
            GitLabJsonContext.Default.CreateProjectHookRequest,
            GitLabJsonContext.Default.GitLabProjectHook,
            cancellationToken);
    }

    public Task<GitLabProjectHook> UpdateAsync(ProjectId projectId, long hookId, UpdateProjectHookRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId).Build(),
            request,
            GitLabJsonContext.Default.UpdateProjectHookRequest,
            GitLabJsonContext.Default.GitLabProjectHook,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long hookId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId).Build(),
            cancellationToken);
    }

    public Task TestAsync(ProjectId projectId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        // Literal, not Escaped: the segment comes from a closed enum, never from caller-supplied text.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId)
                .Literal("test").Literal(trigger.ToRouteValue()).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(ProjectId projectId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId)
                .Literal("events").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabHookEventArray,
            cancellationToken);
    }

    public Task ResendEventAsync(ProjectId projectId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId)
                .Literal("events").Segment(hookLogId).Literal("resend").Build(),
            cancellationToken);
    }

    public Task DeleteUrlVariableAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId)
                .Literal("url_variables").Escaped(key).Build(),
            cancellationToken);
    }

    public Task DeleteCustomHeaderAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("hooks").Segment(hookId)
                .Literal("custom_headers").Escaped(key).Build(),
            cancellationToken);
    }
}