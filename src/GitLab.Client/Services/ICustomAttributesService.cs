using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Custom attributes, sitting between the public
///     <c>ICustomAttributesClient</c> controller and <c>ICustomAttributesRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is
///     the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface ICustomAttributesService
{
    IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForUserAsync(long userId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForUserAsync(long userId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForUserAsync(long userId, string key, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCustomAttribute> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForGroupAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForGroupAsync(GroupId groupId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string key, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCustomAttribute> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForProjectAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForProjectAsync(ProjectId projectId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, string key, CancellationToken cancellationToken = default);
}