using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class SamlGroupLinksClient(IGitLabApiConnection connection) : ISamlGroupLinksClient
{
    public IAsyncEnumerable<GitLabSamlGroupLink> ListAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml_group_links").Build(),
            GitLabJsonContext.Default.GitLabSamlGroupLinkArray,
            cancellationToken);
    }

    public Task<GitLabSamlGroupLink> GetAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml_group_links").Escaped(samlGroupName)
                .Query("provider", provider).Build(),
            GitLabJsonContext.Default.GitLabSamlGroupLink,
            cancellationToken);
    }

    public Task<GitLabSamlGroupLink> CreateAsync(GroupId groupId, CreateSamlGroupLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml_group_links").Build(),
            request,
            GitLabJsonContext.Default.CreateSamlGroupLinkRequest,
            GitLabJsonContext.Default.GitLabSamlGroupLink,
            cancellationToken);
    }

    public Task DeleteAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml_group_links").Escaped(samlGroupName)
                .Query("provider", provider).Build(),
            cancellationToken);
    }
}