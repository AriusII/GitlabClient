using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class RemoteMirrorsClient(IGitLabApiConnection connection) : IRemoteMirrorsClient
{
    public IAsyncEnumerable<GitLabRemoteMirror> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Build(),
            GitLabJsonContext.Default.GitLabRemoteMirrorArray,
            cancellationToken);
    }

    public Task<GitLabRemoteMirror> GetAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Segment(mirrorId)
                .Build(),
            GitLabJsonContext.Default.GitLabRemoteMirror,
            cancellationToken);
    }

    public Task<GitLabRemoteMirror> CreateAsync(ProjectId projectId, CreateRemoteMirrorRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateRemoteMirrorRequest,
            GitLabJsonContext.Default.GitLabRemoteMirror,
            cancellationToken);
    }

    public Task<GitLabRemoteMirror> UpdateAsync(ProjectId projectId, long mirrorId,
        UpdateRemoteMirrorRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Segment(mirrorId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateRemoteMirrorRequest,
            GitLabJsonContext.Default.GitLabRemoteMirror,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long mirrorId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Segment(mirrorId)
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Forces an immediate push. The endpoint answers <c>204 No Content</c>, so this goes through the
    ///     body-less <see cref="IGitLabApiConnection.PostAsync(Uri, CancellationToken)" /> rather than the
    ///     action overload that expects the updated resource back.
    /// </summary>
    public Task SyncAsync(ProjectId projectId, long mirrorId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Segment(mirrorId)
                .Literal("sync")
                .Build(),
            cancellationToken);
    }

    public async Task<GitLabRemoteMirrorPublicKey> GetPublicKeyAsync(ProjectId projectId, long mirrorId,
        CancellationToken cancellationToken = default)
    {
        JsonElement response = await connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("remote_mirrors")
                .Segment(mirrorId)
                .Literal("public_key")
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken).ConfigureAwait(false);

        if (response.ValueKind != JsonValueKind.Object ||
            !response.TryGetProperty("public_key", out JsonElement publicKey) ||
            publicKey.ValueKind != JsonValueKind.String ||
            publicKey.GetString() is not { } value)
        {
            throw new JsonException("GitLab returned an invalid remote-mirror public-key response.");
        }

        return new GitLabRemoteMirrorPublicKey { PublicKey = value };
    }
}