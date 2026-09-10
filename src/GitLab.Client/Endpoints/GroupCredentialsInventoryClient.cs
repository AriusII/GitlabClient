using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class GroupCredentialsInventoryClient(IGitLabApiConnection connection)
    : IGroupCredentialsInventoryClient
{
    private const string ManageSegment = "manage";

    private const string PersonalAccessTokensRoot = "personal_access_tokens";

    private const string ResourceAccessTokensRoot = "resource_access_tokens";

    private const string SshKeysRoot = "ssh_keys";

    private const string RotateSegment = "rotate";

    public IAsyncEnumerable<GitLabPersonalAccessToken> ListPersonalAccessTokensAsync(GroupId groupId,
        PersonalAccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(PersonalAccessTokensRoot).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabPersonalAccessTokenArray,
            cancellationToken);
    }

    public Task RevokePersonalAccessTokenAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(PersonalAccessTokensRoot).Segment(tokenId).Build(),
            cancellationToken);
    }

    public Task<GitLabPersonalAccessTokenWithSecret> RotatePersonalAccessTokenAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(PersonalAccessTokensRoot).Segment(tokenId).Literal(RotateSegment).Build(),
            request,
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            GitLabJsonContext.Default.GitLabPersonalAccessTokenWithSecret,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAccessToken> ListResourceAccessTokensAsync(GroupId groupId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(ResourceAccessTokensRoot).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabAccessTokenArray,
            cancellationToken);
    }

    public Task RevokeResourceAccessTokenAsync(GroupId groupId, long tokenId, DateOnly? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(ResourceAccessTokensRoot).Segment(tokenId).Query("expires_at", expiresAt).Build(),
            cancellationToken);
    }

    public Task RotateResourceAccessTokenAsync(GroupId groupId, long tokenId, RotateAccessTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment)
                .Literal(ResourceAccessTokensRoot).Segment(tokenId).Literal(RotateSegment).Build(),
            request,
            GitLabJsonContext.Default.RotateAccessTokenRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupManagedSshKey> ListSshKeysAsync(GroupId groupId,
        GroupManagedSshKeyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment).Literal(SshKeysRoot)
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabGroupManagedSshKeyArray,
            cancellationToken);
    }

    public Task DeleteSshKeyAsync(GroupId groupId, long keyId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(ManageSegment).Literal(SshKeysRoot)
                .Segment(keyId).Build(),
            cancellationToken);
    }
}