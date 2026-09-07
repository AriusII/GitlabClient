using System.Diagnostics.CodeAnalysis;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Badges resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Project and group badges are separate GitLab route families with identical shapes, so the
///         methods come in <c>...ForProjectAsync</c> / <c>...ForGroupAsync</c> pairs.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IBadgesService), typeof(IBadgesClient))]
internal interface IBadgesRepository
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
        Justification =
            "Badge URLs may contain GitLab placeholders such as %{project_path}, and '%{' is not a valid "
            + "percent-escape, so System.Uri cannot round-trip the values this endpoint exists to render.")]
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
        Justification = "See PreviewForProjectAsync: badge URLs carry placeholders System.Uri cannot represent.")]
    Task<GitLabBadgePreview> PreviewForGroupAsync(GroupId groupId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default);
}