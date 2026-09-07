using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Avatars, sitting between the public <c>IAvatarsClient</c>
///     controller and <c>IAvatarsRepository</c>'s raw GitLab access. Mirrors the repository's method shapes
///     1:1 today (its implementation is generated); this is the seam where request validation, caching, or
///     cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IAvatarsService
{
    Task<GitLabAvatar> GetForEmailAsync(string email, int? size = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}