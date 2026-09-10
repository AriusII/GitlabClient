using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class MobilePushSubscriptionsEndpointTests
{
    private const string SubscriptionJson = """
                                            {
                                              "id": 7,
                                              "created_at": "2026-07-30T12:00:00.000Z"
                                            }
                                            """;

    [Fact]
    public async Task RegisterAsync_PostsToPushSubscriptionsRoute_WithEnumsAsWireValues()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SubscriptionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MobilePushSubscriptionsClient repository = new(connection);

        RegisterMobilePushSubscriptionRequest request = new()
        {
            DeviceToken = "abc123",
            Platform = GitLabMobileDevicePlatform.Ios,
            ApnsEnvironment = GitLabPushSubscriptionApnsEnvironment.Sandbox,
            BundleId = "com.example.app",
            DeviceName = "iPhone",
            AppVersion = "1.2.3",
            Locale = "en-US",
            PayloadMode = GitLabPushSubscriptionPayloadMode.IdOnly
        };

        GitLabMobilePushSubscription subscription =
            await repository.RegisterAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/push_subscriptions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"device_token":"abc123","platform":"ios","apns_environment":"sandbox","bundle_id":"com.example.app","device_name":"iPhone","app_version":"1.2.3","locale":"en-US","payload_mode":"id_only"}
            """,
            sentBody);

        Assert.Equal(7, subscription.Id);
        Assert.Equal(new DateTimeOffset(2026, 7, 30, 12, 0, 0, TimeSpan.Zero), subscription.CreatedAt);
    }

    [Fact]
    public async Task RegisterAsync_OmitsUnsetOptionalMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SubscriptionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MobilePushSubscriptionsClient repository = new(connection);

        RegisterMobilePushSubscriptionRequest request = new() { DeviceToken = "abc123" };

        await repository.RegisterAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("""{"device_token":"abc123"}""", sentBody);
    }

    [Fact]
    public async Task UnregisterAsync_SendsDelete_WithDeviceTokenAsQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MobilePushSubscriptionsClient repository = new(connection);

        await repository.UnregisterAsync("abc 123", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/push_subscriptions?device_token=abc%20123",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UnregisterAsync_OnUnknownToken_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MobilePushSubscriptionsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.UnregisterAsync("missing-token", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}