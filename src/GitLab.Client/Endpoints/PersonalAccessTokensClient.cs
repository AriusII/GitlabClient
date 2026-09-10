using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class PersonalAccessTokensClient(IGitLabApiConnection connection)
    : IPersonalAccessTokensClient
{
    /// <summary>
    ///     GitLab's pseudo-identifier for "the token that authenticated this request". It is a fixed path
    ///     word from the route template, not caller-supplied text, so it goes through
    ///     <see cref="GitLabRouteBuilder.Literal" />.
    /// </summary>
    private const string SelfSegment = "self";

    private const string Root = "personal_access_tokens";

    private const string RotateSegment = "rotate";

    private const string ServiceAccountsSegment = "service_accounts";

    private const string ImpersonationRoot = "impersonation_tokens";

    public IAsyncEnumerable<GitLabPersonalAccessToken> ListAsync(PersonalAccessTokenListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessTokenArray,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessToken> GetAsync(long tokenId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Root)
                .Segment(tokenId)
                .Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessToken,
            cancellationToken);
    }

    public Task RevokeAsync(long tokenId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(Root)
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> RotateAsync(long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        // Rotate is a POST with a JSON body ({"expires_at": ...}), not one of GitLab's empty-bodied
        // action endpoints, so it goes through PostAsync<TRequest, TResponse>. Omitting the body
        // entirely means "let GitLab pick the default expiry", which serializes to {}.
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Root)
                .Segment(tokenId)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessToken> GetSelfAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Root)
                .Literal(SelfSegment)
                .Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessToken,
            cancellationToken);
    }

    public Task RevokeSelfAsync(CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(Root)
                .Literal(SelfSegment)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> RotateSelfAsync(RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Root)
                .Literal(SelfSegment)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabTokenAssociations> GetSelfAssociationsAsync(TokenAssociationListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Not GetPagedAsync: this route answers with a single { "groups": [...], "projects": [...] }
        // object rather than a JSON array, so there is nothing for the Link-header walker to stream.
        // Paging is therefore explicit, through TokenAssociationListOptions.Page.
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Root)
                .Literal(SelfSegment)
                .Literal("associations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabTokenAssociations,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> CreateForCurrentUserAsync(
        CreateCurrentUserPersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Singular "user" - GitLab's route for the caller's own account, distinct from the plural
        // "users/:user_id" administrator route below.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user")
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateCurrentUserPersonalAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> CreateForUserAsync(long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreatePersonalAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPersonalAccessToken> ListForProjectServiceAccountAsync(ProjectId projectId,
        long userId, AccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessTokenArray,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> CreateForProjectServiceAccountAsync(ProjectId projectId,
        long userId, CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreatePersonalAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> RotateForProjectServiceAccountAsync(ProjectId projectId,
        long userId, long tokenId, RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Segment(tokenId)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task RevokeForProjectServiceAccountAsync(ProjectId projectId, long userId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPersonalAccessToken> ListForGroupServiceAccountAsync(GroupId groupId,
        long userId, AccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessTokenArray,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> CreateForGroupServiceAccountAsync(GroupId groupId, long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Build(),
            request,
            GitLabJsonContext.Default.CreatePersonalAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> RotateForGroupServiceAccountAsync(GroupId groupId, long userId,
        long tokenId, RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Segment(tokenId)
                .Literal(RotateSegment)
                .Build(),
            request ?? new RotateAccessTokenRequest(),
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public Task RevokeForGroupServiceAccountAsync(GroupId groupId, long userId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(ServiceAccountsSegment)
                .Segment(userId)
                .Literal(Root)
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabImpersonationToken> ListImpersonationTokensAsync(long userId,
        ImpersonationTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(ImpersonationRoot)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabImpersonationTokenArray,
            cancellationToken);
    }

    public Task<GitLabImpersonationToken> GetImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(ImpersonationRoot)
                .Segment(impersonationTokenId)
                .Build(),
            GitLabJsonContext.Default.GitLabImpersonationToken,
            cancellationToken);
    }

    public Task<GitLabImpersonationTokenWithSecret> CreateImpersonationTokenAsync(long userId,
        CreateImpersonationTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(ImpersonationRoot)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateImpersonationTokenRequest,
            GitLabJsonContext.Default.GitLabImpersonationTokenWithSecret,
            cancellationToken);
    }

    public Task RevokeImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("users")
                .Segment(userId)
                .Literal(ImpersonationRoot)
                .Segment(impersonationTokenId)
                .Build(),
            cancellationToken);
    }
}