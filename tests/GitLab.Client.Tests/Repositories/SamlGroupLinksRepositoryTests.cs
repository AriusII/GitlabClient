using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SamlGroupLinksRepositoryTests
{
    private const string SamlGroupLinkJson = """
                                             {
                                               "name": "saml-group-1",
                                               "access_level": 40,
                                               "member_role_id": 12,
                                               "provider": "saml"
                                             }
                                             """;

    [Fact]
    public async Task ListAsync_BuildsGroupRoute_AndDeserializesTheLink()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SamlGroupLinkJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        List<GitLabSamlGroupLink> links = [];
        await foreach (GitLabSamlGroupLink item in repository.ListAsync(33, TestContext.Current.CancellationToken))
        {
            links.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSamlGroupLink link = Assert.Single(links);
        Assert.Equal("saml-group-1", link.Name);
        Assert.Equal(40, link.AccessLevel);
        Assert.Equal(12, link.MemberRoleId);
        Assert.Equal("saml", link.Provider);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await foreach (GitLabSamlGroupLink unused in
                       repository.ListAsync("parent-group/subgroup", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/saml_group_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EscapesTheSamlGroupName_AndPassesTheProviderAsAQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SamlGroupLinkJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        GitLabSamlGroupLink link = await repository.GetAsync(33, "Engineering/Backend Team", "azure",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links/Engineering%2FBackend%20Team" +
                     "?provider=azure", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("saml-group-1", link.Name);
    }

    [Fact]
    public async Task GetAsync_OmitsTheProviderWhenItWasNotSupplied()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SamlGroupLinkJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await repository.GetAsync(33, "saml-group-1", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links/saml-group-1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheLink()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SamlGroupLinkJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        CreateSamlGroupLinkRequest request = new()
        {
            SamlGroupName = "saml-group-1", AccessLevel = 40, MemberRoleId = 12, Provider = "saml"
        };

        GitLabSamlGroupLink link = await repository.CreateAsync(33, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"saml_group_name":"saml-group-1","access_level":40,"member_role_id":12,"provider":"saml"}
            """,
            sentBody);
        Assert.Equal(40, link.AccessLevel);
    }

    [Fact]
    public async Task CreateAsync_OmitsTheOptionalMembersItWasNotGiven()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SamlGroupLinkJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await repository.CreateAsync(33, new CreateSamlGroupLinkRequest { SamlGroupName = "devs", AccessLevel = 30 },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"saml_group_name":"devs","access_level":30}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_EscapesTheNameAndCarriesTheProvider()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await repository.DeleteAsync(33, "Engineering/Backend Team", "azure",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links/Engineering%2FBackend%20Team" +
                     "?provider=azure", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OmitsTheProviderWhenItWasNotSupplied()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await repository.DeleteAsync(33, "saml-group-1", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml_group_links/saml-group-1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_SurfacesTheAmbiguousLinkErrorAsAValidationException()
    {
        // With more than one SAML provider configured, a name-only lookup is answered with 422 asking
        // for the provider - a validation failure, not a missing resource.
        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent("""{"message":"This group has multiple SAML group links"}""",
                    Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SamlGroupLinksRepository repository = new(connection);

        await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.GetAsync(33, "saml-group-1", cancellationToken: TestContext.Current.CancellationToken));
    }
}