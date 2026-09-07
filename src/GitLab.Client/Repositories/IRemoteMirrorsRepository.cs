using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the RemoteMirrors resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IRemoteMirrorsService), typeof(IRemoteMirrorsClient))]
internal interface IRemoteMirrorsRepository
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

    /// <summary>
    ///     Retrieves the public key of a remote mirror that uses SSH authentication. The spec declares no
    ///     response schema for this route, so the answer is a raw <see cref="JsonElement" /> rather than an
    ///     invented shape.
    /// </summary>
    Task<JsonElement> GetPublicKeyAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default);
}