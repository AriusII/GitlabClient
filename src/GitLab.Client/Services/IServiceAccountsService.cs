using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Service accounts, sitting between the public
///     <c>IServiceAccountsClient</c> controller and <c>IServiceAccountsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IServiceAccountsService
{
    IAsyncEnumerable<GitLabServiceAccount> ListAsync(ServiceAccountListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateAsync(CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateAsync(long userId, UpdateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabServiceAccount> ListForGroupAsync(GroupId groupId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> GetForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateForGroupAsync(GroupId groupId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateForGroupAsync(GroupId groupId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabServiceAccount> ListForProjectAsync(ProjectId projectId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> GetForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateForProjectAsync(ProjectId projectId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateForProjectAsync(ProjectId projectId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);
}