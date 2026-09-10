using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class JiraConnectEndpointTests
{
    private const string SuccessJson = """{"success":true}""";

    private const string SubscriptionJson = """
                                            {
                                              "created_at": "2024-03-11T09:12:44.123Z",
                                              "unlink_path": "/-/jira_connect/subscriptions/42",
                                              "group": {
                                                "name": "Sub Group",
                                                "full_name": "Parent / Sub Group",
                                                "description": "The team namespace",
                                                "avatar_url": "https://gitlab.example/uploads/-/system/group/avatar/8/logo.png"
                                              }
                                            }
                                            """;

    private static HttpResponseMessage Json(HttpStatusCode status, string json)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    private static JiraConnectClient CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        return new JiraConnectClient(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task SubscribeNamespaceAsync_PostsToJiraConnectRoute_AndSendsJwtAndNamespacePath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, SuccessJson);
        });

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabJiraConnectResult result = await repository.SubscribeNamespaceAsync(
                new SubscribeJiraConnectNamespaceRequest
                {
                    Jwt = "eyJhbGciOiJIUzI1NiJ9.payload.sig", NamespacePath = "parent/child"
                },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_connect/subscriptions",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            // A body field, not a route segment: the namespace path reaches GitLab unencoded.
            Assert.Equal("""{"jwt":"eyJhbGciOiJIUzI1NiJ9.payload.sig","namespace_path":"parent/child"}""", sentBody);

            Assert.Equal(JsonValueKind.True, result.Success?.ValueKind);
        }
    }

    [Fact]
    public async Task ListForgeSubscriptionsAsync_BuildsForgeSubscriptionsRoute_AndDeserializesNestedGroup()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, $"[{SubscriptionJson}]"));

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabJiraConnectSubscription> subscriptions = [];
            await foreach (GitLabJiraConnectSubscription item in
                           repository.ListForgeSubscriptionsAsync(TestContext.Current.CancellationToken))
            {
                subscriptions.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_forge/subscriptions",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabJiraConnectSubscription subscription = Assert.Single(subscriptions);
            Assert.Equal(new DateTimeOffset(2024, 3, 11, 9, 12, 44, 123, TimeSpan.Zero), subscription.CreatedAt);
            Assert.Equal("/-/jira_connect/subscriptions/42", subscription.UnlinkPath);
            Assert.Equal("Sub Group", subscription.Group?.Name);
            Assert.Equal("Parent / Sub Group", subscription.Group?.FullName);
            Assert.Equal("The team namespace", subscription.Group?.Description);
            Assert.Equal(new Uri("https://gitlab.example/uploads/-/system/group/avatar/8/logo.png"),
                subscription.Group?.AvatarUrl);
        }
    }

    [Fact]
    public async Task ListForgeSubscriptionsAsync_ToleratesASubscriptionWithoutAGroup()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.OK, """[{"unlink_path":"/-/jira_connect/subscriptions/7"}]"""));

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabJiraConnectSubscription> subscriptions = [];
            await foreach (GitLabJiraConnectSubscription item in
                           repository.ListForgeSubscriptionsAsync(TestContext.Current.CancellationToken))
            {
                subscriptions.Add(item);
            }

            GitLabJiraConnectSubscription subscription = Assert.Single(subscriptions);
            Assert.Null(subscription.Group);
            Assert.Null(subscription.CreatedAt);
        }
    }

    [Fact]
    public async Task CreateForgeSubscriptionAsync_PostsToForgeSubscriptionsRoute_WithoutAJwt()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, SuccessJson);
        });

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.CreateForgeSubscriptionAsync(
                new CreateJiraForgeSubscriptionRequest { NamespacePath = "gitlab-org" },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_forge/subscriptions",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("""{"namespace_path":"gitlab-org"}""", sentBody);
        }
    }

    [Fact]
    public async Task DeleteForgeSubscriptionAsync_BuildsIdRoute_AndReadsTheResponseBody()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, SuccessJson));

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabJiraConnectResult result =
                await repository.DeleteForgeSubscriptionAsync(42, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_forge/subscriptions/42",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(JsonValueKind.True, result.Success?.ValueKind);
        }
    }

    [Fact]
    public async Task UpdateForgeInstallationAsync_PutsInstanceUrlToTheInstallationRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, SuccessJson);
        });

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.UpdateForgeInstallationAsync(
                new UpdateJiraForgeInstallationRequest { InstanceUrl = new Uri("https://gitlab.self-managed.test") },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_forge/installation",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("""{"instance_url":"https://gitlab.self-managed.test"}""", sentBody);
        }
    }

    [Fact]
    public async Task UpdateForgeInstallationAsync_OmitsANullInstanceUrl_WhichGitLabReadsAsGitLabCom()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, SuccessJson);
        });

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.UpdateForgeInstallationAsync(new UpdateJiraForgeInstallationRequest(),
                TestContext.Current.CancellationToken);

            Assert.Equal("{}", sentBody);
        }
    }

    [Fact]
    public async Task RegisterForgeTokenAsync_PostsToForgeTokenRoute_WithNoBody()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return Json(HttpStatusCode.Created, SuccessJson);
        });

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.RegisterForgeTokenAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/integrations/jira_forge/installation/forge_token",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            // GitLab reads the Forge system token from a request header, so the body carries nothing.
            Assert.False(hadContent);
        }
    }

    [Fact]
    public async Task SubscribeNamespaceAsync_WithoutAValidJwt_ThrowsAuthenticationException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Unauthorized, """{"message":"401 Unauthorized"}"""));

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<GitLabAuthenticationException>(() => repository.SubscribeNamespaceAsync(
                new SubscribeJiraConnectNamespaceRequest { Jwt = "expired", NamespacePath = "gitlab-org" },
                TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GitLabJiraConnectResult_ReadsAnObjectShapedSuccessMarkerToo()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.OK, """{"success":{"id":9}}"""));

        JiraConnectClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabJiraConnectResult result =
                await repository.DeleteForgeSubscriptionAsync(9, TestContext.Current.CancellationToken);

            Assert.Equal(JsonValueKind.Object, result.Success?.ValueKind);
        }
    }
}