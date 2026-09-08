using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for artifact attestations, sitting between the public
///     <c>IAttestationsClient</c> controller and <c>IAttestationsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IAttestationsService
{
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default);
}