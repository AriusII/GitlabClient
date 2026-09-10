using System.Diagnostics.CodeAnalysis;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Badges" API area (<c>/projects/:id/badges</c> and <c>/groups/:id/badges</c>) -
///     the pipeline, coverage and custom badges rendered on a project's overview page.
///     <para>
///         Badge URLs are carried as <c>string</c>, not <see cref="Uri" />, because an unrendered badge URL
///         may contain GitLab placeholders (<c>%{project_path}</c>, <c>%{default_branch}</c>,
///         <c>%{commit_sha}</c>) that are not valid percent-escapes. The <c>Rendered*</c> members of the
///         response, which are always fully resolved, are <see cref="Uri" />.
///     </para>
/// </summary>
public interface IBadgesClient
{
    /// <summary>
    ///     Streams a project's badges (<c>GET /projects/:id/badges</c>), including the ones inherited from its
    ///     groups - use <see cref="GitLabBadge.Kind" /> to tell them apart.
    /// </summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="name">Returns only the badges with this exact name.</param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabBadge> ListForProjectAsync(ProjectId projectId, string? name = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one of a project's badges (<c>GET /projects/:id/badges/:badge_id</c>).</summary>
    Task<GitLabBadge> GetForProjectAsync(ProjectId projectId, long badgeId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a badge to a project (<c>POST /projects/:id/badges</c>).</summary>
    Task<GitLabBadge> CreateForProjectAsync(ProjectId projectId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one of a project's badges (<c>PUT /projects/:id/badges/:badge_id</c>).</summary>
    Task<GitLabBadge> UpdateForProjectAsync(ProjectId projectId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one of a project's badges (<c>DELETE /projects/:id/badges/:badge_id</c>).</summary>
    Task DeleteForProjectAsync(ProjectId projectId, long badgeId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Previews how a project would render a badge's URLs without saving anything
    ///     (<c>GET /projects/:id/badges/render</c>).
    /// </summary>
    /// <param name="projectId">The project the placeholders are resolved against.</param>
    /// <param name="linkUrl">The link target to render; may contain GitLab placeholders.</param>
    /// <param name="imageUrl">The image source to render; may contain GitLab placeholders.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    [SuppressMessage("Design", "CA1054",
        Justification =
            "Badge URLs may contain GitLab placeholders such as %{project_path}, and '%{' is not a valid "
            + "percent-escape, so System.Uri cannot round-trip the values this endpoint exists to render.")]
    Task<GitLabBadgePreview> PreviewForProjectAsync(ProjectId projectId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Streams a group's badges (<c>GET /groups/:id/badges</c>).</summary>
    IAsyncEnumerable<GitLabBadge> ListForGroupAsync(GroupId groupId, string? name = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one of a group's badges (<c>GET /groups/:id/badges/:badge_id</c>).</summary>
    Task<GitLabBadge> GetForGroupAsync(GroupId groupId, long badgeId, CancellationToken cancellationToken = default);

    /// <summary>Adds a badge to a group (<c>POST /groups/:id/badges</c>), inherited by every project under it.</summary>
    Task<GitLabBadge> CreateForGroupAsync(GroupId groupId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one of a group's badges (<c>PUT /groups/:id/badges/:badge_id</c>).</summary>
    Task<GitLabBadge> UpdateForGroupAsync(GroupId groupId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one of a group's badges (<c>DELETE /groups/:id/badges/:badge_id</c>).</summary>
    Task DeleteForGroupAsync(GroupId groupId, long badgeId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Previews how a group would render a badge's URLs without saving anything
    ///     (<c>GET /groups/:id/badges/render</c>).
    /// </summary>
    [SuppressMessage("Design", "CA1054",
        Justification = "See PreviewForProjectAsync: badge URLs carry GitLab placeholders System.Uri cannot "
                        + "represent.")]
    Task<GitLabBadgePreview> PreviewForGroupAsync(GroupId groupId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default);
}