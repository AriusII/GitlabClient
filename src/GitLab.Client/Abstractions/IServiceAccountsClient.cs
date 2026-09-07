using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Service accounts" API area (<c>/service_accounts</c>,
///     <c>/groups/:id/service_accounts</c> and <c>/projects/:id/service_accounts</c>) - machine users that own
///     automation credentials without consuming a human seat.
///     <para>
///         The three scopes are the same resource at three ownership levels, so the shapes match: instance
///         accounts are administered by an instance administrator, group accounts by a group Owner, project
///         accounts by a project Owner or Maintainer. The instance scope deliberately has no get-one or delete
///         route - GitLab does not publish them; use <see cref="IUsersClient" /> to read a service account by
///         user id, or delete it through the group or project that owns it.
///     </para>
///     <para>
///         A service account's tokens are not here. They live on
///         <see cref="IPersonalAccessTokensClient.ListForGroupServiceAccountAsync" /> and its project and
///         create/rotate/revoke siblings, which take the same <c>userId</c> as
///         <see cref="GitLabServiceAccount.Id" />.
///     </para>
///     <para>Service accounts are a Premium and Ultimate feature; a Free instance answers <c>403</c>.</para>
/// </summary>
public interface IServiceAccountsClient
{
    /// <summary>Streams every service account on the instance. Requires instance administrator rights.</summary>
    IAsyncEnumerable<GitLabServiceAccount> ListAsync(ServiceAccountListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an instance-level service account. GitLab generates a username, display name and no-reply
    ///     email address for whichever members the request leaves out.
    /// </summary>
    Task<GitLabServiceAccount> CreateAsync(CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an instance-level service account's name, username or email.</summary>
    Task<GitLabServiceAccount> UpdateAsync(long userId, UpdateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every service account owned by a group.</summary>
    IAsyncEnumerable<GitLabServiceAccount> ListForGroupAsync(GroupId groupId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one service account owned by a group, by its user id.</summary>
    Task<GitLabServiceAccount> GetForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a service account owned by a group. Only a top-level group may own one.</summary>
    Task<GitLabServiceAccount> CreateForGroupAsync(GroupId groupId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group service account's name, username or email.</summary>
    Task<GitLabServiceAccount> UpdateForGroupAsync(GroupId groupId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a group service account. Pass <paramref name="hardDelete" /> as <see langword="true" /> to
    ///     remove the account's contributions as well instead of moving them to the Ghost User.
    /// </summary>
    Task DeleteForGroupAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every service account owned by a project.</summary>
    IAsyncEnumerable<GitLabServiceAccount> ListForProjectAsync(ProjectId projectId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one service account owned by a project, by its user id.</summary>
    Task<GitLabServiceAccount> GetForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a service account owned by a project.</summary>
    Task<GitLabServiceAccount> CreateForProjectAsync(ProjectId projectId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a project service account's name, username or email.</summary>
    Task<GitLabServiceAccount> UpdateForProjectAsync(ProjectId projectId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a project service account. Pass <paramref name="hardDelete" /> as <see langword="true" /> to
    ///     remove the account's contributions as well instead of moving them to the Ghost User.
    /// </summary>
    Task DeleteForProjectAsync(ProjectId projectId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);
}