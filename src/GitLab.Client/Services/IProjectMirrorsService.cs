using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Project mirrors, sitting between the public
///     <c>IProjectMirrorsClient</c> controller and <c>IProjectMirrorsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IProjectMirrorsService
{
    Task<GitLabPullMirror> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task StartAsync(ProjectId projectId, TriggerPullMirrorRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPullMirror> UpdateAsync(ProjectId projectId, UpdatePullMirrorRequest request,
        CancellationToken cancellationToken = default);
}