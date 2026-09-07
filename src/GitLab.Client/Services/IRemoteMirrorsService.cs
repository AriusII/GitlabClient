using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for RemoteMirrors, sitting between the public
///     <c>IRemoteMirrorsClient</c> controller and <c>IRemoteMirrorsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IRemoteMirrorsService
{
    IAsyncEnumerable<GitLabRemoteMirror> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabRemoteMirror> GetAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default);

    Task<GitLabRemoteMirror> CreateAsync(ProjectId projectId, CreateRemoteMirrorRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRemoteMirror> UpdateAsync(ProjectId projectId, long mirrorId, UpdateRemoteMirrorRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long mirrorId, CancellationToken cancellationToken = default);

    Task SyncAsync(ProjectId projectId, long mirrorId, CancellationToken cancellationToken = default);

    Task<JsonElement> GetPublicKeyAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default);
}