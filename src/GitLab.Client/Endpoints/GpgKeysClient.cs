using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class GpgKeysClient(IGitLabApiConnection connection) : IGpgKeysClient
{
    /// <summary>
    ///     The singular root: GitLab's alias for the account the request is authenticated as. One letter
    ///     apart from <see cref="NamedUserRoot" />, and operating on the wrong one silently touches the
    ///     wrong account - which is why the two are named constants rather than inline literals.
    /// </summary>
    private const string CurrentUserRoot = "user";

    /// <summary>The plural root, addressing a user by numeric ID.</summary>
    private const string NamedUserRoot = "users";

    private const string GpgKeys = "gpg_keys";

    private const string Revoke = "revoke";

    public IAsyncEnumerable<GitLabGpgKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(GpgKeys)
                .Build(),
            GitLabJsonContext.Default.GitLabGpgKeyArray,
            cancellationToken);
    }

    public Task<GitLabGpgKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Build(),
            GitLabJsonContext.Default.GitLabGpgKey,
            cancellationToken);
    }

    public Task<GitLabGpgKey> CreateForCurrentUserAsync(CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(GpgKeys)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateGpgKeyRequest,
            GitLabJsonContext.Default.GitLabGpgKey,
            cancellationToken);
    }

    public Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Build(),
            cancellationToken);
    }

    public Task RevokeForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default)
    {
        // 202 Accepted with an empty body, so the body-less POST overload rather than the deserializing one.
        return connection.PostAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Literal(Revoke)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGpgKey> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(GpgKeys)
                .Build(),
            GitLabJsonContext.Default.GitLabGpgKeyArray,
            cancellationToken);
    }

    public Task<GitLabGpgKey> GetForUserAsync(long userId, long keyId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Build(),
            GitLabJsonContext.Default.GitLabGpgKey,
            cancellationToken);
    }

    public Task<GitLabGpgKey> CreateForUserAsync(long userId, CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(GpgKeys)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateGpgKeyRequest,
            GitLabJsonContext.Default.GitLabGpgKey,
            cancellationToken);
    }

    public Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Build(),
            cancellationToken);
    }

    public Task RevokeForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(GpgKeys)
                .Segment(keyId)
                .Literal(Revoke)
                .Build(),
            cancellationToken);
    }
}