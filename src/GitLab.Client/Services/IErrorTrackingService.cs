using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Error tracking, sitting between the public
///     <c>IErrorTrackingClient</c> controller and <c>IErrorTrackingRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IErrorTrackingService
{
    IAsyncEnumerable<GitLabErrorTrackingClientKey> ListClientKeysAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingClientKey> CreateClientKeyAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingClientKey> DeleteClientKeyAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> CreateSettingsAsync(ProjectId projectId,
        CreateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);
}