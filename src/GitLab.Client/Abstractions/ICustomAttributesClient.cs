using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Custom attributes" API area (<c>/users/:id/custom_attributes</c>,
///     <c>/groups/:id/custom_attributes</c>, <c>/projects/:id/custom_attributes</c>) - arbitrary key/value
///     metadata an administrator can hang off a user, a group or a project.
///     <para>
///         The three parents share one endpoint shape, so the methods come in matching
///         <c>…ForUser</c>/<c>…ForGroup</c>/<c>…ForProject</c> families that differ only in the parent id.
///     </para>
///     <para>
///         Every operation here, reads included, is administrator-only: a non-administrator token gets
///         <see cref="Exceptions.GitLabForbiddenException" />. Attributes are also hidden from the parent
///         resource's own payload unless it was fetched with <c>with_custom_attributes=true</c>.
///     </para>
/// </summary>
public interface ICustomAttributesClient
{
    /// <summary>Streams every custom attribute set on a user identified by numeric ID.</summary>
    IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every custom attribute set on a user identified by the string form GitLab accepts
    ///     (<c>GET /users/:id/custom_attributes</c>). Pass the raw value; it is encoded as one route segment.
    /// </summary>
    IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(string userIdOrUsername,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one of a user's custom attributes by key. Keys are free text and may contain <c>/</c>; the
    ///     route builder percent-encodes the key, so pass it raw.
    /// </summary>
    Task<GitLabCustomAttribute> GetForUserAsync(long userId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one custom attribute for a user identified by GitLab's string <c>:id</c> form.</summary>
    Task<GitLabCustomAttribute> GetForUserAsync(string userIdOrUsername, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or overwrites one of a user's custom attributes. An upsert - GitLab answers <c>200</c>
    ///     whether the key was new or not.
    /// </summary>
    Task<GitLabCustomAttribute> SetForUserAsync(long userId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or overwrites one custom attribute for a user identified by GitLab's string <c>:id</c> form.
    /// </summary>
    Task<GitLabCustomAttribute> SetForUserAsync(string userIdOrUsername, string key,
        SetCustomAttributeRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a user's custom attributes.</summary>
    Task DeleteForUserAsync(long userId, string key, CancellationToken cancellationToken = default);

    /// <summary>Deletes one custom attribute for a user identified by GitLab's string <c>:id</c> form.</summary>
    Task DeleteForUserAsync(string userIdOrUsername, string key, CancellationToken cancellationToken = default);

    /// <summary>Streams every custom attribute set on a group.</summary>
    IAsyncEnumerable<GitLabCustomAttribute> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one of a group's custom attributes by key.</summary>
    Task<GitLabCustomAttribute> GetForGroupAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or overwrites one of a group's custom attributes.</summary>
    Task<GitLabCustomAttribute> SetForGroupAsync(GroupId groupId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a group's custom attributes.</summary>
    Task DeleteForGroupAsync(GroupId groupId, string key, CancellationToken cancellationToken = default);

    /// <summary>Streams every custom attribute set on a project.</summary>
    IAsyncEnumerable<GitLabCustomAttribute> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one of a project's custom attributes by key.</summary>
    Task<GitLabCustomAttribute> GetForProjectAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or overwrites one of a project's custom attributes.</summary>
    Task<GitLabCustomAttribute> SetForProjectAsync(ProjectId projectId, string key,
        SetCustomAttributeRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a project's custom attributes.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, string key, CancellationToken cancellationToken = default);
}