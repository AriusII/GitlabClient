using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     The Integrations area is 168 spec operations that collapse into ten generic ones, so what these
///     tests guard is the collapsing: that a slug reaches the path escaped, that project and group scope
///     produce different routes, and that an integration this library has no type for still round-trips
///     through the untyped <c>properties</c> object.
/// </summary>
public sealed class IntegrationsEndpointTests
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

    // Keep the 51 typed project convenience methods tied to the authoritative slug table. The generic
    // SetAsync overload deliberately makes a new GitLab integration reachable before this package adds
    // a convenience method; this table protects the opposite direction: a typed request must never
    // silently be wired to the wrong dedicated route or lose its source-generated type metadata.
    private static IReadOnlyList<TypedProjectIntegrationSetter> TypedProjectIntegrationSetters { get; } =
    [
        new(GitLabIntegrationSlug.AppleAppStore,
            static (client, cancellationToken) => client.SetAppleAppStoreAsync(1,
                CreateEmptySettings<AppleAppStoreIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Asana,
            static (client, cancellationToken) =>
                client.SetAsanaAsync(1, CreateEmptySettings<AsanaIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Assembla,
            static (client, cancellationToken) => client.SetAssemblaAsync(1,
                CreateEmptySettings<AssemblaIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Bamboo,
            static (client, cancellationToken) =>
                client.SetBambooAsync(1, CreateEmptySettings<BambooIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Bugzilla,
            static (client, cancellationToken) => client.SetBugzillaAsync(1,
                CreateEmptySettings<BugzillaIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Buildkite,
            static (client, cancellationToken) => client.SetBuildkiteAsync(1,
                CreateEmptySettings<BuildkiteIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Campfire,
            static (client, cancellationToken) => client.SetCampfireAsync(1,
                CreateEmptySettings<CampfireIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.ClickUp,
            static (client, cancellationToken) =>
                client.SetClickUpAsync(1, CreateEmptySettings<ClickUpIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Confluence,
            static (client, cancellationToken) => client.SetConfluenceAsync(1,
                CreateEmptySettings<ConfluenceIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.CustomIssueTracker,
            static (client, cancellationToken) => client.SetCustomIssueTrackerAsync(1,
                CreateEmptySettings<CustomIssueTrackerIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Datadog,
            static (client, cancellationToken) =>
                client.SetDatadogAsync(1, CreateEmptySettings<DatadogIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.DiffblueCover,
            static (client, cancellationToken) => client.SetDiffblueCoverAsync(1,
                CreateEmptySettings<DiffblueCoverIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Discord,
            static (client, cancellationToken) =>
                client.SetDiscordAsync(1, CreateEmptySettings<DiscordIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.DroneCi,
            static (client, cancellationToken) =>
                client.SetDroneCiAsync(1, CreateEmptySettings<DroneCiIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.EmailsOnPush,
            static (client, cancellationToken) => client.SetEmailsOnPushAsync(1,
                CreateEmptySettings<EmailsOnPushIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Ewm,
            static (client, cancellationToken) =>
                client.SetEwmAsync(1, CreateEmptySettings<EwmIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.ExternalWiki,
            static (client, cancellationToken) => client.SetExternalWikiAsync(1,
                CreateEmptySettings<ExternalWikiIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.GitGuardian,
            static (client, cancellationToken) => client.SetGitGuardianAsync(1,
                CreateEmptySettings<GitGuardianIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.GitHub,
            static (client, cancellationToken) =>
                client.SetGitHubAsync(1, CreateEmptySettings<GitHubIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.GitLabSlackApplication,
            static (client, cancellationToken) => client.SetGitLabSlackApplicationAsync(1,
                CreateEmptySettings<GitLabSlackApplicationIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.GoogleCloudPlatformArtifactRegistry,
            static (client, cancellationToken) => client.SetGoogleCloudPlatformArtifactRegistryAsync(1,
                CreateEmptySettings<GoogleCloudPlatformArtifactRegistryIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.GoogleCloudPlatformWorkloadIdentityFederation,
            static (client, cancellationToken) => client.SetGoogleCloudPlatformWorkloadIdentityFederationAsync(1,
                CreateEmptySettings<GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest>(),
                cancellationToken)),
        new(GitLabIntegrationSlug.GooglePlay,
            static (client, cancellationToken) => client.SetGooglePlayAsync(1,
                CreateEmptySettings<GooglePlayIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.HangoutsChat,
            static (client, cancellationToken) => client.SetHangoutsChatAsync(1,
                CreateEmptySettings<HangoutsChatIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Harbor,
            static (client, cancellationToken) =>
                client.SetHarborAsync(1, CreateEmptySettings<HarborIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Irker,
            static (client, cancellationToken) =>
                client.SetIrkerAsync(1, CreateEmptySettings<IrkerIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Jenkins,
            static (client, cancellationToken) =>
                client.SetJenkinsAsync(1, CreateEmptySettings<JenkinsIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Jira,
            static (client, cancellationToken) =>
                client.SetJiraAsync(1, CreateEmptySettings<JiraIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.JiraCloudApp,
            static (client, cancellationToken) => client.SetJiraCloudAppAsync(1,
                CreateEmptySettings<JiraCloudAppIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Linear,
            static (client, cancellationToken) =>
                client.SetLinearAsync(1, CreateEmptySettings<LinearIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Matrix,
            static (client, cancellationToken) =>
                client.SetMatrixAsync(1, CreateEmptySettings<MatrixIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Mattermost,
            static (client, cancellationToken) => client.SetMattermostAsync(1,
                CreateEmptySettings<MattermostIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.MattermostSlashCommands,
            static (client, cancellationToken) => client.SetMattermostSlashCommandsAsync(1,
                CreateEmptySettings<MattermostSlashCommandsIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.MicrosoftTeams,
            static (client, cancellationToken) => client.SetMicrosoftTeamsAsync(1,
                CreateEmptySettings<MicrosoftTeamsIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.MockCi,
            static (client, cancellationToken) =>
                client.SetMockCiAsync(1, CreateEmptySettings<MockCiIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.MockMonitoring,
            static (client, cancellationToken) => client.SetMockMonitoringAsync(1,
                CreateEmptySettings<MockMonitoringIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Packagist,
            static (client, cancellationToken) => client.SetPackagistAsync(1,
                CreateEmptySettings<PackagistIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Phorge,
            static (client, cancellationToken) =>
                client.SetPhorgeAsync(1, CreateEmptySettings<PhorgeIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.PipelinesEmail,
            static (client, cancellationToken) => client.SetPipelinesEmailAsync(1,
                CreateEmptySettings<PipelinesEmailIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.PivotalTracker,
            static (client, cancellationToken) => client.SetPivotalTrackerAsync(1,
                CreateEmptySettings<PivotalTrackerIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Pumble,
            static (client, cancellationToken) =>
                client.SetPumbleAsync(1, CreateEmptySettings<PumbleIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Pushover,
            static (client, cancellationToken) => client.SetPushoverAsync(1,
                CreateEmptySettings<PushoverIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Redmine,
            static (client, cancellationToken) =>
                client.SetRedmineAsync(1, CreateEmptySettings<RedmineIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Slack,
            static (client, cancellationToken) =>
                client.SetSlackAsync(1, CreateEmptySettings<SlackIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.SquashTm,
            static (client, cancellationToken) => client.SetSquashTmAsync(1,
                CreateEmptySettings<SquashTmIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.TeamCity,
            static (client, cancellationToken) => client.SetTeamCityAsync(1,
                CreateEmptySettings<TeamCityIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Telegram,
            static (client, cancellationToken) => client.SetTelegramAsync(1,
                CreateEmptySettings<TelegramIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.UnifyCircuit,
            static (client, cancellationToken) => client.SetUnifyCircuitAsync(1,
                CreateEmptySettings<UnifyCircuitIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.WebexTeams,
            static (client, cancellationToken) => client.SetWebexTeamsAsync(1,
                CreateEmptySettings<WebexTeamsIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.YouTrack,
            static (client, cancellationToken) => client.SetYouTrackAsync(1,
                CreateEmptySettings<YouTrackIntegrationRequest>(), cancellationToken)),
        new(GitLabIntegrationSlug.Zentao,
            static (client, cancellationToken) =>
                client.SetZentaoAsync(1, CreateEmptySettings<ZentaoIntegrationRequest>(), cancellationToken))
    ];

    [Fact]
    public async Task ListAsync_BuildsTheProjectIntegrationsRoute_AndDeserializesTheBasicEntity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicIntegrationJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

        await repository.SetForGroupAsync("parent-group/subgroup", GitLabIntegrationSlug.MicrosoftTeams,
            new TestSlackSettings { Webhook = new Uri("https://example.test/hook") },
            GitLabTestJsonContext.Default.TestSlackSettings, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/integrations/microsoft-teams",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task TypedProjectSetters_CoverEveryGitLab19Slug_AndUseTheirDedicatedRoutes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BasicIntegrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsClient client = new(connection);

        Assert.Equal(51, TypedProjectIntegrationSetters.Count);

        string[] declaredSlugs = typeof(GitLabIntegrationSlug)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(static field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(static field => (string)field.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] typedSetterSlugs = TypedProjectIntegrationSetters
            .Select(static setter => setter.Slug)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(declaredSlugs, typedSetterSlugs);

        foreach (TypedProjectIntegrationSetter setter in TypedProjectIntegrationSetters)
        {
            // Required members are intentionally left unset here. This is a route-and-source-metadata
            // test, not a request validation test; each delegate must exercise its exact typed setter.
            await setter.Invoke(client, TestContext.Current.CancellationToken).ConfigureAwait(true);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal($"https://gitlab.example/api/v4/projects/1/integrations/{setter.Slug}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task DisableAsync_SendsDeleteToTheProjectSlugRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

        await repository.ProcessSlackInteractionAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/integrations/slack/interactions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ProcessSlackOptionsAsync_PostsToTheOptionsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsClient repository = new(connection);

        await repository.ProcessSlackOptionsAsync(TestContext.Current.CancellationToken);

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
        IntegrationsClient repository = new(connection);

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
        IntegrationsClient repository = new(connection);

        await repository.TriggerMattermostSlashCommandAsync("gitlab-org/gitlab",
            new TriggerMattermostSlashCommandRequest { Token = "s3cr3t" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // GitLab spells the slug with underscores on the trigger route and with hyphens everywhere else.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/integrations/mattermost_slash_commands/trigger",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"token":"s3cr3t"}""", sentBody);
    }

    private static TSettings CreateEmptySettings<TSettings>()
        where TSettings : class
    {
        return (TSettings)RuntimeHelpers.GetUninitializedObject(typeof(TSettings));
    }

    private sealed record TypedProjectIntegrationSetter(
        string Slug,
        Func<IntegrationsClient, CancellationToken, Task<GitLabIntegration>> Invoke);
}