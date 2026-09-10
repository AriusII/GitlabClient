using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>Direct, allocation-light transport mapping for GitLab's LDAP directory and group-link routes.</summary>
internal sealed class LdapClient(IGitLabApiConnection connection) : ILdapClient
{
    public IAsyncEnumerable<GitLabLdapGroup> ListGroupsAsync(string? search = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("ldap").Literal("groups").Query("search", search).Build(),
            GitLabJsonContext.Default.GitLabLdapGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabLdapGroup> ListGroupsForProviderAsync(string provider, string? search = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("ldap").Escaped(provider).Literal("groups").Query("search", search).Build(),
            GitLabJsonContext.Default.GitLabLdapGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupLdapLink> ListGroupLinksAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupLinksRoute(groupId).Build(),
            GitLabJsonContext.Default.GitLabGroupLdapLinkArray,
            cancellationToken);
    }

    public Task<GitLabGroupLdapLink> CreateGroupLinkAsync(GroupId groupId, CreateLdapGroupLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupLinksRoute(groupId).Build(),
            request,
            GitLabJsonContext.Default.CreateLdapGroupLinkRequest,
            GitLabJsonContext.Default.GitLabGroupLdapLink,
            cancellationToken);
    }

    public Task DeleteGroupLinkAsync(GroupId groupId, DeleteLdapGroupLinkOptions options,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(GroupLinksRoute(groupId).QueryFrom(options).Build(), cancellationToken);
    }

    public Task SynchronizeGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("ldap_sync").Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder GroupLinksRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("ldap_group_links");
    }
}