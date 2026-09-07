using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Provider identities, sitting between the public
///     <c>IProviderIdentitiesClient</c> controller and <c>IProviderIdentitiesRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once the resource needs more than pass-through.
/// </summary>
internal interface IProviderIdentitiesService
{
    IAsyncEnumerable<GitLabProviderIdentity> ListSamlAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> GetSamlAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> UpdateSamlAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    Task DeleteSamlAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProviderIdentity> ListScimAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> GetScimAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> UpdateScimAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    Task DeleteScimAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);
}