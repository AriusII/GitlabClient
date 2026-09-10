using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "System hooks" API area (<c>/hooks</c>) - instance-wide webhooks that fire for
///     project, group, user and key lifecycle events across the whole GitLab installation.
///     <para>
///         This is an administrator surface: every endpoint here requires instance admin rights and answers
///         <c>403</c> otherwise, and none of it applies to gitlab.com. The trigger set is narrower than for
///         project or group hooks - see <see cref="GitLabSystemHook" />.
///     </para>
///     <para>
///         Secrets travel one way only: a hook's <c>token</c>, <c>signing_token</c>, URL-variable values
///         and custom-header values can be written but are never read back, so
///         <see cref="GitLabSystemHook" /> reports only whether they are set.
///     </para>
/// </summary>
public interface ISystemHooksClient
{
    /// <summary>Streams every system hook registered on the instance.</summary>
    IAsyncEnumerable<GitLabSystemHook> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one system hook by id.</summary>
    Task<GitLabSystemHook> GetAsync(long hookId, CancellationToken cancellationToken = default);

    /// <summary>Registers a system hook.</summary>
    Task<GitLabSystemHook> CreateAsync(CreateSystemHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a system hook. Unset properties on the request are omitted rather than cleared, but
    ///     <see cref="UpdateSystemHookRequest.UrlVariables" /> and
    ///     <see cref="UpdateSystemHookRequest.CustomHeaders" /> replace their collections wholesale when set.
    /// </summary>
    Task<GitLabSystemHook> UpdateAsync(long hookId, UpdateSystemHookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a system hook.</summary>
    Task DeleteAsync(long hookId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fires a test run of the hook with mock data. Unlike project and group hooks, the system-hook test
    ///     takes no trigger: GitLab posts to the hook's own route (<c>POST /hooks/:hook_id</c>).
    /// </summary>
    Task TestAsync(long hookId, CancellationToken cancellationToken = default);

    /// <summary>Removes one URL variable from the hook, leaving the others in place.</summary>
    Task DeleteUrlVariableAsync(long hookId, string key, CancellationToken cancellationToken = default);

    /// <summary>Removes one custom header from the hook, leaving the others in place.</summary>
    Task DeleteCustomHeaderAsync(long hookId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one URL variable's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateUrlVariableAsync(long hookId, string key, UpdateSystemHookUrlVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets one custom header's value, leaving the others in place. GitLab answers <c>200</c> with no
    ///     body and never echoes the value back.
    /// </summary>
    Task UpdateCustomHeaderAsync(long hookId, string key, UpdateSystemHookCustomHeaderRequest request,
        CancellationToken cancellationToken = default);
}