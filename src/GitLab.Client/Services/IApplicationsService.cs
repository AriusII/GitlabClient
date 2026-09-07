using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Applications, sitting between the public
///     <c>IApplicationsClient</c> controller and <c>IApplicationsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IApplicationsService
{
    IAsyncEnumerable<GitLabApplication> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> RenewSecretAsync(long id, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabApplication> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateForCurrentUserAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApplication> GetForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplication> UpdateForCurrentUserAsync(long id, UpdateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForCurrentUserAsync(long id, CancellationToken cancellationToken = default);
}