using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ProviderIdentitiesEndpointTests
{
    private const string IdentityJson = """
                                        {
                                          "extern_uid": "4",
                                          "user_id": 48,
                                          "active": true
                                        }
                                        """;

    [Fact]
    public async Task ListSamlAsync_BuildsSamlIdentitiesRoute_AndDeserializesTheIdentity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{IdentityJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        List<GitLabProviderIdentity> identities = [];
        await foreach (GitLabProviderIdentity item in
                       repository.ListSamlAsync(33, TestContext.Current.CancellationToken))
        {
            identities.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml/identities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProviderIdentity identity = Assert.Single(identities);
        Assert.Equal("4", identity.ExternUid);
        Assert.Equal(48, identity.UserId);
        Assert.True(identity.Active);
    }

    [Fact]
    public async Task ListScimAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        await foreach (GitLabProviderIdentity unused in
                       repository.ListScimAsync("parent-group/subgroup", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/scim/identities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetSamlAsync_EscapesTheExternUid()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(IdentityJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        GitLabProviderIdentity identity =
            await repository.GetSamlAsync(33, "CN=jane doe,OU=Users/DC=example",
                TestContext.Current.CancellationToken);

        // An extern_uid is opaque provider-supplied text: it routinely carries '/', ',', '=' and
        // spaces. Every one of those has to be percent-encoded into a single path segment, which is
        // why the route uses Escaped() and not Literal().
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/33/saml/CN%3Djane%20doe%2COU%3DUsers%2FDC%3Dexample",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("4", identity.ExternUid);
    }

    [Fact]
    public async Task GetScimAsync_EscapesTheExternUid()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(IdentityJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        GitLabProviderIdentity identity =
            await repository.GetScimAsync(33, "jane.doe@example.com/1", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/33/scim/jane.doe%40example.com%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(48, identity.UserId);
    }

    [Fact]
    public async Task UpdateSamlAsync_PatchesTheNewExternUid()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"extern_uid":"new-uid","user_id":48,"active":true}""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        GitLabProviderIdentity identity = await repository.UpdateSamlAsync(33, "old/uid",
            new UpdateProviderIdentityRequest { ExternUid = "new-uid" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml/old%2Fuid",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"extern_uid":"new-uid"}""", sentBody);
        Assert.Equal("new-uid", identity.ExternUid);
    }

    [Fact]
    public async Task UpdateScimAsync_PatchesTheScimRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IdentityJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        await repository.UpdateScimAsync("parent-group/subgroup", "old-uid",
            new UpdateProviderIdentityRequest { ExternUid = "new-uid" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/scim/old-uid",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"extern_uid":"new-uid"}""", sentBody);
    }

    [Fact]
    public async Task DeleteSamlAsync_SendsDeleteToTheEscapedUid()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        await repository.DeleteSamlAsync(33, "some/uid", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/saml/some%2Fuid",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteScimAsync_ToleratesGitLabEchoingTheDeletedIdentity()
    {
        // The spec declares 200-with-body for this delete while GitLab answers 204 in practice, so the
        // repository ignores the body either way rather than failing on the empty one.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(IdentityJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        await repository.DeleteScimAsync(33, "4", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/33/scim/4",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetSamlAsync_SurfacesNotFoundAsTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Not found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetSamlAsync(33, "missing", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetSamlAsync_ReadsAUserIdGitLabSentAsAString()
    {
        // The pinned spec types every member of the identity payload as a string (Grape's default for
        // an undocumented exposure) while GitLab actually sends user_id as a number. The context's
        // NumberHandling.AllowReadingFromString means either shape deserializes.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"extern_uid":"4","user_id":"48"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProviderIdentitiesClient repository = new(connection);

        GitLabProviderIdentity identity =
            await repository.GetSamlAsync(33, "4", TestContext.Current.CancellationToken);

        Assert.Equal(48, identity.UserId);
        Assert.Null(identity.Active);
    }
}