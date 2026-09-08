using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Project hooks" API area (<c>/projects/:id/hooks</c>) - project-level webhooks,
///     their delivery log, and the URL-variable and custom-header sub-resources.
///     <para>
///         Every endpoint here requires Maintainer on the project. Secrets travel one way only: a hook's
///         <c>token</c>, <c>signing_token</c>, URL-variable values and custom-header values can be written
///         but are never read back, so <see cref="GitLabProjectHook" /> reports only whether they are set.
///     </para>
/// </summary>
public interface IProjectHooksClient
{
    /// <summary>Streams every webhook configured on a project.</summary>
    IAsyncEnumerable<GitLabProjectHook> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Gets one webhook by id.</summary>
    Task<GitLabProjectHook> GetAsync(ProjectId projectId, long hookId, CancellationToken cancellationToken = default);

    /// <summary>Adds a webhook to a project.</summary>
    Task<GitLabProjectHook> AddAsync(ProjectId projectId, CreateProjectHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a webhook. Unset properties on the request are omitted rather than cleared, but
    ///     <see cref="UpdateProjectHookRequest.UrlVariables" /> and
    ///     <see cref="UpdateProjectHookRequest.CustomHeaders" /> replace their collections wholesale when set.
    /// </summary>
    Task<GitLabProjectHook> UpdateAsync(ProjectId projectId, long hookId, UpdateProjectHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a webhook.</summary>
    Task DeleteAsync(ProjectId projectId, long hookId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fires a test delivery of the given event type against the hook, using mock data. GitLab rate-limits
    ///     this to five requests per minute per user per project and answers <c>429</c> beyond that.
    /// </summary>
    Task TestAsync(ProjectId projectId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the hook's delivery log, newest first. GitLab retains entries for seven days, so an empty
    ///     result means nothing was delivered recently rather than nothing ever was.
    /// </summary>
    IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(ProjectId projectId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replays one logged delivery, identified by <see cref="GitLabHookEvent.Id" />. Rate-limited the same
    ///     way as <see cref="TestAsync" />.
    /// </summary>
    Task ResendEventAsync(ProjectId projectId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one URL variable from the hook, leaving the others in place.</summary>
    Task DeleteUrlVariableAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one custom header from the hook, leaving the others in place.</summary>
    Task DeleteCustomHeaderAsync(ProjectId projectId, long hookId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one URL variable's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateUrlVariableAsync(ProjectId projectId, long hookId, string key,
        UpdateProjectHookUrlVariableRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one custom header's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateCustomHeaderAsync(ProjectId projectId, long hookId, string key,
        UpdateProjectHookCustomHeaderRequest request, CancellationToken cancellationToken = default);
}