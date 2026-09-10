using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Group hooks" API area (<c>/groups/:id/hooks</c>) - group-level webhooks, their
///     delivery log, and the URL-variable and custom-header sub-resources.
///     <para>
///         A group hook fires for activity anywhere beneath the group, including its subgroups and their
///         projects, which is what makes it the alternative to registering the same
///         <see cref="IProjectHooksClient" /> hook on every project. Group hooks are a Premium feature: on
///         Free, these endpoints answer <c>404</c> rather than <c>403</c>.
///     </para>
///     <para>
///         Secrets travel one way only: a hook's <c>token</c>, <c>signing_token</c>, URL-variable values
///         and custom-header values can be written but are never read back, so
///         <see cref="GitLabGroupHook" /> reports only whether they are set.
///     </para>
/// </summary>
public interface IGroupHooksClient
{
    /// <summary>Streams every webhook configured on a group.</summary>
    IAsyncEnumerable<GitLabGroupHook> ListAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Gets one webhook by id.</summary>
    Task<GitLabGroupHook> GetAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default);

    /// <summary>Adds a webhook to a group.</summary>
    Task<GitLabGroupHook> CreateAsync(GroupId groupId, CreateGroupHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a webhook. Unset properties on the request are omitted rather than cleared, but
    ///     <see cref="UpdateGroupHookRequest.UrlVariables" /> and
    ///     <see cref="UpdateGroupHookRequest.CustomHeaders" /> replace their collections wholesale when set.
    /// </summary>
    Task<GitLabGroupHook> UpdateAsync(GroupId groupId, long hookId, UpdateGroupHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a webhook.</summary>
    Task DeleteAsync(GroupId groupId, long hookId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fires a test delivery of the given event type against the hook, using mock data. GitLab rate-limits
    ///     this to five requests per minute per user per group and answers <c>429</c> beyond that.
    /// </summary>
    Task TestAsync(GroupId groupId, long hookId, GitLabWebhookTestTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the hook's delivery log, newest first. GitLab retains entries for seven days, so an empty
    ///     result means nothing was delivered recently rather than nothing ever was.
    /// </summary>
    IAsyncEnumerable<GitLabHookEvent> ListEventsAsync(GroupId groupId, long hookId,
        HookEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replays one logged delivery, identified by <see cref="GitLabHookEvent.Id" />. Rate-limited the same
    ///     way as <see cref="TestAsync" />.
    /// </summary>
    Task ResendEventAsync(GroupId groupId, long hookId, long hookLogId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one URL variable from the hook, leaving the others in place.</summary>
    Task DeleteUrlVariableAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one custom header from the hook, leaving the others in place.</summary>
    Task DeleteCustomHeaderAsync(GroupId groupId, long hookId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one URL variable's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateUrlVariableAsync(GroupId groupId, long hookId, string key, UpdateGroupHookUrlVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one custom header's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateCustomHeaderAsync(GroupId groupId, long hookId, string key, UpdateGroupHookCustomHeaderRequest request,
        CancellationToken cancellationToken = default);
}