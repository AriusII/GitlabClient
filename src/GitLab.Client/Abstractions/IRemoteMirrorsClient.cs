using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Remote mirrors" API area (<c>/projects/:id/remote_mirrors</c>) - push mirrors only;
///     pull mirroring lives on a different endpoint.
/// </summary>
public interface IRemoteMirrorsClient
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

    /// <summary>Forces an immediate update of the mirror (<c>POST .../remote_mirrors/:mirror_id/sync</c>).</summary>
    Task SyncAsync(ProjectId projectId, long mirrorId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the public key of a remote mirror that uses SSH authentication
    ///     (<c>GET .../remote_mirrors/:mirror_id/public_key</c>). The spec declares no response schema for
    ///     this route, so the answer is a raw <see cref="JsonElement" /> rather than an invented shape.
    /// </summary>
    Task<JsonElement> GetPublicKeyAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default);
}