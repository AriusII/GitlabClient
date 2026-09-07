using System.Diagnostics.CodeAnalysis;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Badges, sitting between the public <c>IBadgesClient</c>
///     controller and <c>IBadgesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IBadgesService
{
    IAsyncEnumerable<GitLabBadge> ListForProjectAsync(ProjectId projectId, string? name = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBadge> GetForProjectAsync(ProjectId projectId, long badgeId,
        CancellationToken cancellationToken = default);

    Task<GitLabBadge> CreateForProjectAsync(ProjectId projectId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBadge> UpdateForProjectAsync(ProjectId projectId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long badgeId, CancellationToken cancellationToken = default);

    [SuppressMessage("Design", "CA1054",
        Justification = "See IBadgesRepository.PreviewForProjectAsync: badge URLs carry GitLab placeholders "
                        + "System.Uri cannot represent.")]
    Task<GitLabBadgePreview> PreviewForProjectAsync(ProjectId projectId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBadge> ListForGroupAsync(GroupId groupId, string? name = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBadge> GetForGroupAsync(GroupId groupId, long badgeId, CancellationToken cancellationToken = default);

    Task<GitLabBadge> CreateForGroupAsync(GroupId groupId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBadge> UpdateForGroupAsync(GroupId groupId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long badgeId, CancellationToken cancellationToken = default);

    [SuppressMessage("Design", "CA1054",
        Justification = "See IBadgesRepository.PreviewForProjectAsync: badge URLs carry GitLab placeholders "
                        + "System.Uri cannot represent.")]
    Task<GitLabBadgePreview> PreviewForGroupAsync(GroupId groupId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default);
}