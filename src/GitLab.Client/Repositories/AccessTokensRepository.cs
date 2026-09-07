using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class AccessTokensRepository(IGitLabApiConnection connection) : IAccessTokensRepository
{
    private const string Root = "access_tokens";

    /// <summary>
    ///     GitLab's pseudo-identifier for "the token that authenticated this request". It is a fixed path
    ///     word from the route template, not caller-supplied text, so it goes through
    ///     <see cref="GitLabRouteBuilder.Literal" />.
    /// </summary>
    private const string SelfSegment = "self";

    private const string RotateSegment = "rotate";

    public IAsyncEnumerable<GitLabAccessToken> ListForProjectAsync(ProjectId projectId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAccessTokenArray,
            cancellationToken);
    }

    public Task<GitLabAccessToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            GitLabJsonContext.Default.GitLabAccessToken,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> CreateForProjectAsync(ProjectId projectId,
        CreateAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> RotateForProjectAsync(ProjectId projectId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        // Rotate is a POST with a JSON body ({"expires_at": ...}), not one of GitLab's empty-bodied
        // action endpoints, so it goes through PostAsync<TRequest, TResponse>. Omitting the body
        // entirely means "let GitLab pick the default expiry", which serializes to {}.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Segment(tokenId)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> RotateSelfForProjectAsync(ProjectId projectId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Literal(SelfSegment)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task RevokeForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAccessToken> ListForGroupAsync(GroupId groupId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAccessTokenArray,
            cancellationToken);
    }

    public Task<GitLabAccessToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            GitLabJsonContext.Default.GitLabAccessToken,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> CreateForGroupAsync(GroupId groupId,
        CreateAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> RotateForGroupAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Segment(tokenId)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabAccessTokenWithSecret> RotateSelfForGroupAsync(GroupId groupId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Literal(SelfSegment)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabAccessTokenWithSecret,
            cancellationToken);
    }

    public Task RevokeForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }
}