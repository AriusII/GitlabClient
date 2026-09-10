using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class LdapEndpointTests
{
    private const string LdapGroupLinkJson = """
                                             {
                                               "cn": "cn=developers,ou=groups,dc=example,dc=com",
                                               "group_access": 30,
                                               "provider": "main",
                                               "filter": "(memberOf=developers)",
                                               "member_role_id": 7
                                             }
                                             """;

    [Fact]
    public async Task ListGroupsAsync_SendsTheSearchToTheInstanceDirectoryRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"cn":"cn=developers,ou=groups,dc=example,dc=com"}]""",
                Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabLdapGroup> groups = [];
        await foreach (GitLabLdapGroup group in client.ListGroupsAsync("developers ops",
                           TestContext.Current.CancellationToken))
        {
            groups.Add(group);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ldap/groups?search=developers%20ops",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("cn=developers,ou=groups,dc=example,dc=com", Assert.Single(groups).Cn);
    }

    [Fact]
    public async Task ListGroupsForProviderAsync_EscapesTheProviderAndOmitsAnUnsetSearch()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabLdapGroup _ in client.ListGroupsForProviderAsync("corporate / ldap",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty directory result.");
        }

        Assert.Equal("https://gitlab.example/api/v4/ldap/corporate%20%2F%20ldap/groups",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListGroupLinksAsync_EncodesTheNamespacedGroupPath_AndMapsTheFullLink()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{LdapGroupLinkJson}]", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabGroupLdapLink> links = [];
        await foreach (GitLabGroupLdapLink link in client.ListGroupLinksAsync("platform/engineering",
                           TestContext.Current.CancellationToken))
        {
            links.Add(link);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/platform%2Fengineering/ldap_group_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGroupLdapLink groupLink = Assert.Single(links);
        Assert.Equal("cn=developers,ou=groups,dc=example,dc=com", groupLink.Cn);
        Assert.Equal(30, groupLink.GroupAccess);
        Assert.Equal("main", groupLink.Provider);
        Assert.Equal("(memberOf=developers)", groupLink.Filter);
        Assert.Equal(7, groupLink.MemberRoleId);
    }

    [Fact]
    public async Task CreateGroupLinkAsync_PostsEveryDeclaredBodyMember()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(LdapGroupLinkJson, Encoding.UTF8, "application/json")
            };
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        GitLabGroupLdapLink link = await client.CreateGroupLinkAsync(41,
            new CreateLdapGroupLinkRequest
            {
                Cn = "cn=developers,ou=groups,dc=example,dc=com",
                GroupAccess = 30,
                Provider = "main",
                MemberRoleId = 7
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/41/ldap_group_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal(
            """
            {"cn":"cn=developers,ou=groups,dc=example,dc=com","group_access":30,"provider":"main","member_role_id":7}
            """,
            sentBody);
        Assert.Equal("main", link.Provider);
    }

    [Fact]
    public async Task CreateGroupLinkAsync_PreservesTheFilterAlternative_AndOmitsUnsetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(LdapGroupLinkJson, Encoding.UTF8, "application/json")
            };
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        await client.CreateGroupLinkAsync(41,
            new CreateLdapGroupLinkRequest
            {
                Filter = "(&(objectClass=groupOfNames)(cn=engineering))", GroupAccess = 40, Provider = "directory-1"
            }, TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        using JsonDocument requestDocument = JsonDocument.Parse(sentBody);
        JsonElement requestBody = requestDocument.RootElement;
        Assert.Equal("(&(objectClass=groupOfNames)(cn=engineering))", requestBody.GetProperty("filter").GetString());
        Assert.Equal(40, requestBody.GetProperty("group_access").GetInt32());
        Assert.Equal("directory-1", requestBody.GetProperty("provider").GetString());
        Assert.Equal(3, requestBody.EnumerateObject().Count());
    }

    [Fact]
    public async Task DeleteGroupLinkAsync_UsesTheCurrentCollectionRoute_AndEscapesTheFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        await client.DeleteGroupLinkAsync("platform/engineering",
            new DeleteLdapGroupLinkOptions
            {
                Provider = "directory / primary", Filter = "(&(cn=engineering)(member=alice@example.com))"
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/platform%2Fengineering/ldap_group_links" +
                     "?provider=directory%20%2F%20primary&filter=%28%26%28cn%3Dengineering%29%28member%3Dalice%40example.com%29%29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SynchronizeGroupAsync_PostsAnEmptyBodyToTheActionRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        LdapClient client = new(new GitLabApiConnection(httpClient));

        await client.SynchronizeGroupAsync(41, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("https://gitlab.example/api/v4/groups/41/ldap_sync", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}