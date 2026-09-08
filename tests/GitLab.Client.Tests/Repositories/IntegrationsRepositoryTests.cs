using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     The Integrations area is 168 spec operations that collapse into ten generic ones, so what these
///     tests guard is the collapsing: that a slug reaches the path escaped, that project and group scope
///     produce different routes, and that an integration this library has no type for still round-trips
///     through the untyped <c>properties</c> object.
/// </summary>
public sealed class IntegrationsRepositoryTests
{
    private const string BasicIntegrationJson = """
                                                {
                                                  "id": 75,
                                                  "title": "Jenkins",
                                                  "slug": "jenkins",
                                                  "created_at": "2019-11-20T11:20:25.297Z",
                                                  "updated_at": "2019-11-20T12:24:37.498Z",
                                                  "active": true,
                                                  "commit_events": true,
                                                  "push_events": true,
                                                  "issues_events": true,
                                                  "incident_events": false,
                                                  "alert_events": true,
                                                  "confidential_issues_events": true,
                                                  "merge_requests_events": true,
                                                  "tag_push_events": false,
                                                  "deployment_events": false,
                                                  "note_events": true,
                                                  "confidential_note_events": true,
                                                  "pipeline_events": true,
                                                  "wiki_page_events": true,
                                                  "job_events": true,
                                                  "comment_on_event_enabled": true,
                                                  "inherited": false,
                                                  "vulnerability_events": false
                                                }
                                                """;

    private const string FullIntegrationJson = """
                                               {
                                                 "id": 75,
                                                 "title": "Jenkins",
                                                 "slug": "jenkins",
                                                 "created_at": "2019-11-20T11:20:25.297Z",
                                                 "updated_at": "2019-11-20T12:24:37.498Z",
                                                 "active": true,
                                                 "inherited": false,
                                                 "commit_events": true,
                                                 "push_events": true,
                                                 "properties": {
                                                   "jenkins_url": "http://jenkins.example.com/",
                                                   "project_name": "my_project_name",
                                                   "username": "admin",
                                                   "enable_ssl_verification": true
                                                 }
                                               }
                                               """;

    [Fact]
    public async Task ListAsync_BuildsTheProjectIntegrationsRoute_AndDeserializesTheBasicEntity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicIntegrationJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        List<GitLabIntegration> integrations = [];
        await foreach (GitLabIntegration item in repository.ListAsync(1, TestContext.Current.CancellationToken))
        {
            integrations.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabIntegration integration = Assert.Single(integrations);
        Assert.Equal(75, integration.Id);
        Assert.Equal("Jenkins", integration.Title);
        Assert.Equal(GitLabIntegrationSlug.Jenkins, integration.Slug);
        Assert.True(integration.Active);
        Assert.False(integration.Inherited);
        Assert.True(integration.CommentOnEventEnabled);
        Assert.True(integration.ConfidentialNoteEvents);
        Assert.False(integration.TagPushEvents);
        Assert.False(integration.VulnerabilityEvents);
        Assert.Equal(new DateTimeOffset(2019, 11, 20, 11, 20, 25, 297, TimeSpan.Zero), integration.CreatedAt);

        // The listing entity is IntegrationBasic: it carries no "properties" member at all.
        Assert.Null(integration.Properties);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsTheGroupRoute_AndEncodesANamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await foreach (GitLabIntegration _ in
                       repository.ListForGroupAsync("parent-group/subgroup", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/integrations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EncodesTheNamespacedProjectPath_AndKeepsAHyphenatedSlugIntact()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", GitLabIntegrationSlug.AppleAppStore,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The project path is percent-encoded to stay one segment; the hyphens in the slug are
        // unreserved characters and must survive Escaped() untouched, or every route in this area 404s.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/integrations/apple-app-store",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EscapesASlugThatIsNotUrlSafe()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        // Every slug GitLab ships today is url-safe, which is exactly why Literal() would look correct
        // here. The slug is caller-supplied text, so it goes through Escaped(): proven with a value that
        // actually needs encoding.
        await repository.GetAsync(1, "custom issue/tracker", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/custom%20issue%2Ftracker",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_SurfacesTheUntypedPropertiesObject()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabIntegration integration =
            await repository.GetAsync(1, GitLabIntegrationSlug.Jenkins, TestContext.Current.CancellationToken);

        Assert.NotNull(integration.Properties);

        JsonElement properties = integration.Properties.Value;
        Assert.Equal(JsonValueKind.Object, properties.ValueKind);
        Assert.Equal("http://jenkins.example.com/", properties.GetProperty("jenkins_url").GetString());
        Assert.Equal("my_project_name", properties.GetProperty("project_name").GetString());

        // The value kinds survive too - a bool stays a bool rather than becoming the string "True".
        Assert.Equal(JsonValueKind.True, properties.GetProperty("enable_ssl_verification").ValueKind);
    }

    [Fact]
    public async Task GetForGroupAsync_EncodesTheNamespacedGroupPath_AndDeserializesTheIntegration()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabIntegration integration = await repository.GetForGroupAsync("parent-group/subgroup",
            GitLabIntegrationSlug.Jenkins, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/integrations/jenkins",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(75, integration.Id);
        Assert.NotNull(integration.Properties);
    }

    [Fact]
    public async Task GetForGroupAsync_OnAnUnconfiguredIntegration_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Service Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForGroupAsync(9970, GitLabIntegrationSlug.Jira, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/integrations/jira",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetAsync_RoundTripsTheUntypedPropertiesObjectBackToGitLab()
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

        // Exactly what a caller with no typed settings record has to hand: the properties object read
        // back off a GET.
        using JsonDocument read = JsonDocument.Parse(FullIntegrationJson);
        Dictionary<string, JsonElement> settings = [];
        foreach (JsonProperty property in read.RootElement.GetProperty("properties").EnumerateObject())
        {
            settings[property.Name] = property.Value;
        }

        GitLabIntegration integration = await repository.SetAsync(1, GitLabIntegrationSlug.Jenkins, settings,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jenkins",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody);

        // Keys are GitLab's own snake_case parameter names and must go out verbatim - the snake_case
        // property policy applies to DTO members, not to dictionary keys.
        Assert.Equal(4, sent.RootElement.EnumerateObject().Count());
        Assert.Equal("http://jenkins.example.com/", sent.RootElement.GetProperty("jenkins_url").GetString());
        Assert.Equal("admin", sent.RootElement.GetProperty("username").GetString());
        Assert.Equal(JsonValueKind.True, sent.RootElement.GetProperty("enable_ssl_verification").ValueKind);

        Assert.Equal(75, integration.Id);
    }

    [Fact]
    public async Task SetForGroupAsync_PutsTheRawPropertyBagToTheGroupRoute()
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

        Dictionary<string, JsonElement> settings = new()
        {
            ["webhook"] = JsonSerializer.SerializeToElement("https://hooks.slack.com/services/x",
                GitLabTestJsonContext.Default.String)
        };

        await repository.SetForGroupAsync(9970, GitLabIntegrationSlug.Slack, settings,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/integrations/slack",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"webhook":"https://hooks.slack.com/services/x"}""", sentBody);
    }

    [Fact]
    public async Task SetAsync_WithACallerSuppliedTypeInfo_SerializesTheTypedSettings()
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

        // This is the shape the per-slug typed setters wrap: a settings record plus its own
        // source-generated JsonTypeInfo, so nothing on this path is reflection-based.
        TestSlackSettings settings = new()
        {
            Webhook = new Uri("https://hooks.slack.com/services/x"), NotifyOnlyBrokenPipelines = true
        };

        await repository.SetAsync(1, GitLabIntegrationSlug.Slack, settings,
            GitLabTestJsonContext.Default.TestSlackSettings, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/slack",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"webhook":"https://hooks.slack.com/services/x","notify_only_broken_pipelines":true}""",
            sentBody);
    }

    [Fact]
    public async Task SetForGroupAsync_WithACallerSuppliedTypeInfo_PutsToTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.SetForGroupAsync("parent-group/subgroup", GitLabIntegrationSlug.MicrosoftTeams,
            new TestSlackSettings { Webhook = new Uri("https://example.test/hook") },
            GitLabTestJsonContext.Default.TestSlackSettings, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/integrations/microsoft-teams",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DisableAsync_SendsDeleteToTheProjectSlugRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.DisableAsync(1, GitLabIntegrationSlug.MattermostSlashCommands,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/mattermost-slash-commands",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DisableForGroupAsync_SendsDeleteToTheGroupSlugRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.DisableForGroupAsync(9970, GitLabIntegrationSlug.CustomIssueTracker,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/integrations/custom-issue-tracker",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnAnUnconfiguredIntegration_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Service Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, GitLabIntegrationSlug.Jira, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jira",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ReceiveSlackEventAsync_PostsToTheInstanceLevelSlackEventsRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.ReceiveSlackEventAsync(
            new SlackEventRequest { Type = "event_callback", TeamId = "T0001", EventTime = 1_732_000_000 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/integrations/slack/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"team_id":"T0001","type":"event_callback","event_time":1732000000}""", sentBody);
    }

    [Fact]
    public async Task ProcessSlackInteractionAsync_PostsAnEmptyBodyToTheInteractionsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.ProcessSlackInteractionAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/integrations/slack/interactions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetSlackOptionsAsync_PostsToTheOptionsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.GetSlackOptionsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/integrations/slack/options",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task TriggerSlackCommandAsync_PostsToTheGlobalSlackTriggerRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.TriggerSlackCommandAsync(new TriggerSlackCommandRequest { Text = "issue show 42" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // Not under /integrations: this one hangs off the API root.
        Assert.Equal("https://gitlab.example/api/v4/slack/trigger", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"text":"issue show 42"}""", sentBody);
    }

    [Fact]
    public async Task TriggerMattermostSlashCommandAsync_UsesTheUnderscoredSlugInThePath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await repository.TriggerMattermostSlashCommandAsync("gitlab-org/gitlab",
            new TriggerMattermostSlashCommandRequest { Token = "s3cr3t" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // GitLab spells the slug with underscores on the trigger route and with hyphens everywhere else.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/integrations/mattermost_slash_commands/trigger",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"token":"s3cr3t"}""", sentBody);
    }
}