using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class CustomAttributesEndpointTests
{
    private const string AttributeJson = """
                                         {
                                           "key": "location",
                                           "value": "Antarctica"
                                         }
                                         """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListForUserAsync_BuildsUserRoute_AndDeserializesThePayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{AttributeJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabCustomAttribute> attributes = [];
        await foreach (GitLabCustomAttribute attribute in
                       repository.ListForUserAsync(42, TestContext.Current.CancellationToken))
        {
            attributes.Add(attribute);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/custom_attributes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabCustomAttribute only = Assert.Single(attributes);
        Assert.Equal("location", only.Key);
        Assert.Equal("Antarctica", only.Value);
    }

    [Fact]
    public async Task ListForUserAsync_StringUserId_EncodesTheRouteSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabCustomAttribute _ in repository.ListForUserAsync("admin user",
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/users/admin%20user/custom_attributes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The rule this whole resource hangs on: an attribute key is free text an administrator typed, so it
    ///     is <c>Escaped</c>, never <c>Literal</c>. Unescaped, "team/owner" would address a route that does
    ///     not exist and come back as a 404 that reads like a missing attribute rather than a client bug.
    /// </summary>
    [Fact]
    public async Task GetForUserAsync_PercentEncodesAKeyContainingASlash()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabCustomAttribute attribute =
            await repository.GetForUserAsync(42, "team/owner", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/42/custom_attributes/team%2Fowner",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("location", attribute.Key);
    }

    [Fact]
    public async Task GetForUserAsync_StringUserId_EncodesBothCallerSuppliedSegments()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.GetForUserAsync("admin user", "team/owner", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/admin%20user/custom_attributes/team%2Fowner",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForGroupAsync_EncodesBothTheNamespacedGroupPathAndTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.GetForGroupAsync("parent-group/subgroup", "cost centre",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/custom_attributes/cost%20centre",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabCustomAttribute _ in
                       repository.ListForProjectAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/custom_attributes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetForProjectAsync_PutsOnlyTheValue_AndReturnsTheStoredAttribute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabCustomAttribute attribute = await repository.SetForProjectAsync(
            7,
            "location",
            new SetCustomAttributeRequest { Value = "Antarctica" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"value":"Antarctica"}""", sentBody);
        Assert.Equal("Antarctica", attribute.Value);
    }

    [Fact]
    public async Task SetForGroupAsync_BuildsTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.SetForGroupAsync(9970, "location",
            new SetCustomAttributeRequest { Value = "Antarctica" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetForUserAsync_BuildsTheUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.SetForUserAsync(42, "location",
            new SetCustomAttributeRequest { Value = "Antarctica" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetForUserAsync_StringUserId_PreservesTheRequiredNullableValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "key": "location", "value": null }""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabCustomAttribute attribute = await repository.SetForUserAsync("admin user", "location",
            new SetCustomAttributeRequest { Value = null }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/admin%20user/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"value":null}""", sentBody);
        Assert.Null(attribute.Value);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTheProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AttributeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.GetForProjectAsync(7, "location", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabCustomAttribute _ in
                       repository.ListForGroupAsync(9970, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/custom_attributes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForUserAsync_SendsDeleteToTheUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForUserAsync(42, "location", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForUserAsync_StringUserId_EncodesTheRouteSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForUserAsync("admin user", "team/owner", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/admin%20user/custom_attributes/team%2Fowner",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForGroupAsync(9970, "location", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToTheProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        CustomAttributesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForProjectAsync(7, "location", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/custom_attributes/location",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}