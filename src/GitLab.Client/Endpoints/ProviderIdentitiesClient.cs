using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProviderIdentitiesClient(IGitLabApiConnection connection) : IProviderIdentitiesClient
{
    public IAsyncEnumerable<GitLabProviderIdentity> ListSamlAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml").Literal("identities").Build(),
            GitLabJsonContext.Default.GitLabProviderIdentityArray,
            cancellationToken);
    }

    public Task<GitLabProviderIdentity> GetSamlAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml").Escaped(externUid).Build(),
            GitLabJsonContext.Default.GitLabProviderIdentity,
            cancellationToken);
    }

    public Task<GitLabProviderIdentity> UpdateSamlAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml").Escaped(externUid).Build(),
            request,
            GitLabJsonContext.Default.UpdateProviderIdentityRequest,
            GitLabJsonContext.Default.GitLabProviderIdentity,
            cancellationToken);
    }

    public Task DeleteSamlAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml").Escaped(externUid).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProviderIdentity> ListScimAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("scim").Literal("identities").Build(),
            GitLabJsonContext.Default.GitLabProviderIdentityArray,
            cancellationToken);
    }

    public Task<GitLabProviderIdentity> GetScimAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("scim").Escaped(externUid).Build(),
            GitLabJsonContext.Default.GitLabProviderIdentity,
            cancellationToken);
    }

    public Task<GitLabProviderIdentity> UpdateScimAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("scim").Escaped(externUid).Build(),
            request,
            GitLabJsonContext.Default.UpdateProviderIdentityRequest,
            GitLabJsonContext.Default.GitLabProviderIdentity,
            cancellationToken);
    }

    public Task DeleteScimAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("scim").Escaped(externUid).Build(),
            cancellationToken);
    }
}