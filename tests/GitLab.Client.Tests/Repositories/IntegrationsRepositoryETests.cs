using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Part E of the Integrations resource: the seven integrations addressed only through GitLab's older
///     <c>/projects/:id/services/...</c> path spelling (Squash TM, TeamCity, Telegram, Unify Circuit,
///     Webex Teams, YouTrack, ZenTao), plus the slug-generic get/disable pair on that same alias. What
///     these tests guard is that every one of those methods reaches <c>/services/...</c>, not
///     <c>/integrations/...</c>, and that each typed settings record serializes its required fields and
///     omits the rest.
/// </summary>
public sealed class IntegrationsRepositoryETests
{
    private const string BasicIntegrationJson = """
                                                {
                                                  "id": 42,
                                                  "title": "Squash TM",
                                                  "slug": "squash-tm",
                                                  "created_at": "2020-01-15T09:30:00.000Z",
                                                  "updated_at": "2020-01-15T09:30:00.000Z",
                                                  "active": true,
                                                  "commit_events": true,
                                                  "push_events": true,
                                                  "issues_events": true,
                                                  "incident_events": false,
                                                  "alert_events": false,
                                                  "confidential_issues_events": false,
                                                  "merge_requests_events": true,
                                                  "tag_push_events": true,
                                                  "deployment_events": false,
                                                  "note_events": true,
                                                  "confidential_note_events": false,
                                                  "pipeline_events": true,
                                                  "wiki_page_events": false,
                                                  "job_events": true,
                                                  "comment_on_event_enabled": false,
                                                  "inherited": false,
                                                  "vulnerability_events": false
                                                }
                                                """;

    private const string FullIntegrationJson = """
                                               {
                                                 "id": 17,
                                                 "title": "Apple App Store",
                                                 "slug": "apple-app-store",
                                                 "created_at": "2021-06-01T08:00:00.000Z",
                                                 "updated_at": "2021-06-02T08:00:00.000Z",
                                                 "active": true,
                                                 "inherited": false,
                                                 "properties": {
                                                   "app_store_issuer_id": "abc-123",
                                                   "app_store_key_id": "key-456"
                                                 }
                                               }
                                               """;

    [Fact]
    public async Task SetSquashTmAsync_BuildsTheServicesAliasRoute_AndOmitsUnsetFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabIntegration integration = await repository.SetSquashTmAsync(1,
            new SquashTmSettingsRequest
            {
                Url = new Uri("https://squash.example.com/hooks/incoming"), IssuesEvents = true
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);

        // The legacy alias, not /integrations/squash-tm.
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/squash-tm",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("""{"url":"https://squash.example.com/hooks/incoming","issues_events":true}""", sentBody);
        Assert.Equal(42, integration.Id);
        Assert.Equal(GitLabIntegrationSlug.SquashTm, integration.Slug);
    }

    [Fact]
    public async Task SetTeamCityAsync_UsesTheCompressedTeamcityUrlWireName()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetTeamCityAsync(1,
            new TeamCitySettingsRequest
            {
                TeamCityUrl = new Uri("https://teamcity.example.com"),
                BuildType = "GitLabBuild_Build",
                Username = "ci-bot",
                Password = "hunter2",
                EnableSslVerification = false
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/teamcity",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The default snake_case policy would produce "team_city_url" for a property named
        // TeamCityUrl; the explicit [JsonPropertyName] is what keeps it "teamcity_url" on the wire.
        Assert.NotNull(sentBody);
        using JsonDocument document = JsonDocument.Parse(sentBody);
        Assert.Equal("https://teamcity.example.com", document.RootElement.GetProperty("teamcity_url").GetString());
        Assert.Equal("GitLabBuild_Build", document.RootElement.GetProperty("build_type").GetString());
        Assert.Equal("ci-bot", document.RootElement.GetProperty("username").GetString());
        Assert.Equal("hunter2", document.RootElement.GetProperty("password").GetString());
        Assert.False(document.RootElement.GetProperty("enable_ssl_verification").GetBoolean());
        Assert.False(document.RootElement.TryGetProperty("push_events", out _));
    }

    [Fact]
    public async Task SetTelegramAsync_SerializesTheRequiredTokenAndRoom()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetTelegramAsync(1,
            new TelegramSettingsRequest
            {
                Hostname = new Uri("https://api.telegram.org"), Token = "123456:ABC-token", Room = "@channelname"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/telegram",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"hostname":"https://api.telegram.org","token":"123456:ABC-token","room":"@channelname"}""",
            sentBody);
    }

    [Fact]
    public async Task SetUnifyCircuitAsync_BuildsTheServicesAliasRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetUnifyCircuitAsync(1,
            new UnifyCircuitSettingsRequest { Webhook = new Uri("https://circuit.com/rest/v2/webhooks/incoming/abc") },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/unify-circuit",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"webhook":"https://circuit.com/rest/v2/webhooks/incoming/abc"}""", sentBody);
    }

    [Fact]
    public async Task SetWebexTeamsAsync_BuildsTheServicesAliasRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetWebexTeamsAsync(1,
            new WebexTeamsSettingsRequest
            {
                Webhook = new Uri("https://api.ciscospark.com/v1/webhooks/incoming/abc"),
                NotifyOnlyBrokenPipelines = true
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/webex-teams",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"webhook":"https://api.ciscospark.com/v1/webhooks/incoming/abc","notify_only_broken_pipelines":true}""",
            sentBody);
    }

    [Fact]
    public async Task SetYouTrackAsync_SerializesBothRequiredUrls()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetYouTrackAsync(1,
            new YouTrackSettingsRequest
            {
                ProjectUrl = new Uri("https://youtrack.example.com/project"),
                IssuesUrl = new Uri("https://youtrack.example.com/issue")
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/youtrack",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"project_url":"https://youtrack.example.com/project","issues_url":"https://youtrack.example.com/issue"}""",
            sentBody);
    }

    [Fact]
    public async Task SetZentaoAsync_SerializesRequiredFields_AndOmitsTheOptionalApiUrl()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetZentaoAsync(1,
            new ZentaoSettingsRequest
            {
                Url = new Uri("https://zentao.example.com"), ApiToken = "tok123", ZentaoProductXid = "42"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/zentao",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"url":"https://zentao.example.com","api_token":"tok123","zentao_product_xid":"42"}""",
            sentBody);
    }

    [Fact]
    public async Task GetServiceAsync_BuildsTheServicesAliasRoute_AndDeserializesTheFullEntity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabIntegration integration = await repository.GetServiceAsync("gitlab-org/gitlab",
            GitLabIntegrationSlug.AppleAppStore, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The legacy alias, not /integrations/apple-app-store; the namespaced project path still has to
        // stay one percent-encoded segment.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/services/apple-app-store",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(17, integration.Id);
        Assert.Equal("apple-app-store", integration.Slug);
        Assert.NotNull(integration.Properties);

        JsonElement properties = integration.Properties.Value;
        Assert.Equal("abc-123", properties.GetProperty("app_store_issuer_id").GetString());
    }

    [Fact]
    public async Task GetServiceAsync_EscapesASlugThatIsNotUrlSafe()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        // The slug is caller-supplied text here (unlike the seven fixed-slug setters above), so it must
        // go through Escaped() - proven with a value that actually needs encoding.
        await repository.GetServiceAsync(1, "custom issue/tracker", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/custom%20issue%2Ftracker",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DisableServiceAsync_SendsDeleteToTheServicesAliasRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.DisableServiceAsync(1, GitLabIntegrationSlug.Bugzilla,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services/bugzilla",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}