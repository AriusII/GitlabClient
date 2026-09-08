using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     The part-B typed per-slug setters added over the generic Integrations "create or update"
///     endpoint: Confluence, the Custom Issue Tracker, Datadog, Diffblue Cover, Discord, Drone CI,
///     Emails on Push, IBM EWM, the External Wiki, GitGuardian, GitHub and the GitLab for Slack app.
///     Each test proves the route (project scope, correct slug, correct verb) and that the typed record
///     serializes to GitLab's own snake_case parameter names.
/// </summary>
public sealed class IntegrationsRepositoryBTests
{
    private const string BasicIntegrationJson = """
                                                {
                                                  "id": 42,
                                                  "title": "Some Integration",
                                                  "slug": "some-integration",
                                                  "created_at": "2024-01-01T00:00:00.000Z",
                                                  "updated_at": "2024-01-02T00:00:00.000Z",
                                                  "active": true,
                                                  "push_events": true
                                                }
                                                """;

    [Fact]
    public async Task SetConfluenceAsync_PutsToTheIntegrationsSlugRoute_AndSendsOnlyTheSetFields()
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

        GitLabIntegration integration = await repository.SetConfluenceAsync(1,
            new ConfluenceIntegrationRequest { ConfluenceUrl = new Uri("https://example.atlassian.net/wiki") },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);

        // Routed under the modern /integrations alias, not the deprecated /services/confluence path
        // the vendored spec also lists this operation under - see IIntegrationsRepository.B.cs.
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/confluence",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Equal("""{"confluence_url":"https://example.atlassian.net/wiki"}""", sentBody);
        Assert.Equal(42, integration.Id);
    }

    [Fact]
    public async Task SetCustomIssueTrackerAsync_SendsAllThreeRequiredUrls()
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

        await repository.SetCustomIssueTrackerAsync("gitlab-org/gitlab",
            new CustomIssueTrackerIntegrationRequest
            {
                ProjectUrl = new Uri("https://example.test/project"),
                IssuesUrl = new Uri("https://example.test/issue/:id"),
                NewIssueUrl = new Uri("https://example.test/issue/new")
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/integrations/custom-issue-tracker",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody);
        Assert.Equal(3, sent.RootElement.EnumerateObject().Count());
        Assert.Equal("https://example.test/project", sent.RootElement.GetProperty("project_url").GetString());
        Assert.Equal("https://example.test/issue/:id", sent.RootElement.GetProperty("issues_url").GetString());
        Assert.Equal("https://example.test/issue/new", sent.RootElement.GetProperty("new_issue_url").GetString());
    }

    [Fact]
    public async Task SetDatadogAsync_SendsTheRequiredApiKey_AndTheOptionalFlagsWhenSet()
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

        await repository.SetDatadogAsync(1,
            new DatadogIntegrationRequest
            {
                ApiKey = "secret-key", DatadogSite = "datadoghq.eu", DatadogCiVisibility = true
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/datadog",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody);
        Assert.Equal("secret-key", sent.RootElement.GetProperty("api_key").GetString());
        Assert.Equal("datadoghq.eu", sent.RootElement.GetProperty("datadog_site").GetString());
        Assert.True(sent.RootElement.GetProperty("datadog_ci_visibility").GetBoolean());

        // Unset optional members are omitted, not sent as null.
        Assert.False(sent.RootElement.TryGetProperty("archive_trace_events", out _));
    }

    [Fact]
    public async Task SetDiffblueCoverAsync_SendsAllThreeRequiredCredentialFields()
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

        await repository.SetDiffblueCoverAsync(1,
            new DiffblueCoverIntegrationRequest
            {
                DiffblueLicenseKey = "lic-123",
                DiffblueAccessTokenName = "ci-token",
                DiffblueAccessTokenSecret = "s3cr3t"
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/diffblue-cover",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Equal(
            """{"diffblue_license_key":"lic-123","diffblue_access_token_name":"ci-token","diffblue_access_token_secret":"s3cr3t"}""",
            sentBody);
    }

    [Fact]
    public async Task SetDiscordAsync_SendsTheWebhook_AndAPerEventChannelOverride()
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

        await repository.SetDiscordAsync(1,
            new DiscordIntegrationRequest
            {
                Webhook = "https://discord.com/api/webhooks/1/token", PushChannel = "commits", PushEvents = true
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/discord",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody);
        Assert.Equal("https://discord.com/api/webhooks/1/token", sent.RootElement.GetProperty("webhook").GetString());
        Assert.Equal("commits", sent.RootElement.GetProperty("push_channel").GetString());
        Assert.True(sent.RootElement.GetProperty("push_events").GetBoolean());
    }

    [Fact]
    public async Task SetDroneCiAsync_SendsTheRequiredUrlAndToken()
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

        await repository.SetDroneCiAsync(1,
            new DroneCiIntegrationRequest { DroneUrl = new Uri("http://drone.example.com"), Token = "drone-token" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/drone-ci",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"drone_url":"http://drone.example.com","token":"drone-token"}""", sentBody);
    }

    [Fact]
    public async Task SetEmailsOnPushAsync_SendsTheRequiredRecipients()
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

        await repository.SetEmailsOnPushAsync(1,
            new EmailsOnPushIntegrationRequest { Recipients = "a@example.test b@example.test" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/emails-on-push",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"recipients":"a@example.test b@example.test"}""", sentBody);
    }

    [Fact]
    public async Task SetEwmAsync_SendsAllThreeRequiredUrls()
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

        await repository.SetEwmAsync(1,
            new EwmIntegrationRequest
            {
                ProjectUrl = new Uri("https://example.test/project"),
                IssuesUrl = new Uri("https://example.test/issue/:id"),
                NewIssueUrl = new Uri("https://example.test/issue/new")
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/ewm",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody);
        Assert.Equal(3, sent.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public async Task SetExternalWikiAsync_SendsTheRequiredUrl()
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

        await repository.SetExternalWikiAsync(1,
            new ExternalWikiIntegrationRequest { ExternalWikiUrl = new Uri("https://wiki.example.test") },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/external-wiki",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"external_wiki_url":"https://wiki.example.test"}""", sentBody);
    }

    [Fact]
    public async Task SetGitGuardianAsync_SendsTheRequiredToken_AndTheOptionalApiUrl()
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

        await repository.SetGitGuardianAsync(1,
            new GitGuardianIntegrationRequest
            {
                Token = "gg-token", ApiUrl = new Uri("https://api.eu1.gitguardian.com")
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/git-guardian",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"api_url":"https://api.eu1.gitguardian.com","token":"gg-token"}""", sentBody);
    }

    [Fact]
    public async Task SetGitHubAsync_SendsTheRequiredTokenAndRepositoryUrl()
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

        await repository.SetGitHubAsync(1,
            new GitHubIntegrationRequest
            {
                Token = "gh-token", RepositoryUrl = new Uri("https://github.com/example/repo")
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/github",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"token":"gh-token","repository_url":"https://github.com/example/repo"}""", sentBody);
    }

    [Fact]
    public async Task SetGitLabSlackApplicationAsync_HasNoRequiredFields_AndSendsOnlyWhatIsSet()
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

        await repository.SetGitLabSlackApplicationAsync(1,
            new GitLabSlackApplicationIntegrationRequest { Channel = "general", NotifyOnlyBrokenPipelines = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/gitlab-slack-application",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"channel":"general","notify_only_broken_pipelines":true}""", sentBody);
    }

    [Fact]
    public async Task SetConfluenceAsync_DeserializesTheReturnedIntegration()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabIntegration integration = await repository.SetConfluenceAsync(1,
            new ConfluenceIntegrationRequest { ConfluenceUrl = new Uri("https://example.atlassian.net/wiki") },
            TestContext.Current.CancellationToken);

        Assert.Equal(42, integration.Id);
        Assert.Equal("some-integration", integration.Slug);
        Assert.True(integration.Active);
        Assert.True(integration.PushEvents);
    }
}