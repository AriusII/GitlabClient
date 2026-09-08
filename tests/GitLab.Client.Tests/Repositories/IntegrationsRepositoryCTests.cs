using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Covers the twelve part-C typed per-slug setters added to <see cref="IntegrationsRepository" />:
///     Google Cloud Platform Artifact Registry, Google Cloud Platform Workload Identity Federation,
///     Google Play, Hangouts Chat, Harbor, Irker, Jenkins, Jira, the Jira Cloud App, Linear, Matrix and
///     Mattermost. Each test proves the same three things the base <c>IntegrationsRepositoryTests</c>
///     proves for the generic setter: the route is <c>/projects/:id/integrations/:slug</c> (the modern
///     spelling, not the deprecated <c>/services/:slug</c> alias), the outbound body carries GitLab's
///     own snake_case keys, and the response deserializes back into <see cref="GitLabIntegration" />.
/// </summary>
public sealed class IntegrationsRepositoryCTests
{
    private const string ResponseJson = """
                                        {
                                          "id": 42,
                                          "title": "Some integration",
                                          "slug": "some-integration",
                                          "active": true,
                                          "push_events": true
                                        }
                                        """;

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification =
            "The HttpClient/handler pair outlives this factory method by design - every test method that " +
            "calls this helper uses it for the whole test body, and disposal has no observable effect here " +
            "since StubHttpMessageHandler holds no unmanaged resources; the process reclaims both at test " +
            "exit.")]
    private static (IntegrationsRepository Repository, StubHttpMessageHandler Handler, Func<string?> SentBody)
        CreateRepository()
    {
        string? sentBody = null;
        StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        return (new IntegrationsRepository(connection), handler, () => sentBody);
    }

    [Fact]
    public async Task SetGoogleCloudPlatformArtifactRegistryAsync_PutsToTheModernIntegrationsRoute()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        GitLabIntegration integration = await repository.SetGoogleCloudPlatformArtifactRegistryAsync(1,
            new GoogleCloudPlatformArtifactRegistryIntegrationRequest
            {
                ArtifactRegistryProjectId = "my-gcp-project",
                ArtifactRegistryRepositories = "my-repo",
                ArtifactRegistryLocation = "us-central1"
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/integrations/google-cloud-platform-artifact-registry",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("my-gcp-project", sent.RootElement.GetProperty("artifact_registry_project_id").GetString());
        Assert.Equal("my-repo", sent.RootElement.GetProperty("artifact_registry_repositories").GetString());
        Assert.Equal("us-central1", sent.RootElement.GetProperty("artifact_registry_location").GetString());
        Assert.False(sent.RootElement.TryGetProperty("use_inherited_settings", out _));

        Assert.Equal(42, integration.Id);
    }

    [Fact]
    public async Task SetGoogleCloudPlatformWorkloadIdentityFederationAsync_SendsAllFourIdentifiers()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetGoogleCloudPlatformWorkloadIdentityFederationAsync(1,
            new GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest
            {
                WorkloadIdentityFederationProjectId = "my-project",
                WorkloadIdentityFederationProjectNumber = "1234567890",
                WorkloadIdentityPoolId = "my-pool",
                WorkloadIdentityPoolProviderId = "my-provider",
                UseInheritedSettings = false
            }, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/integrations/google-cloud-platform-workload-identity-federation",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("my-project",
            sent.RootElement.GetProperty("workload_identity_federation_project_id").GetString());
        Assert.Equal("1234567890",
            sent.RootElement.GetProperty("workload_identity_federation_project_number").GetString());
        Assert.Equal("my-pool", sent.RootElement.GetProperty("workload_identity_pool_id").GetString());
        Assert.Equal("my-provider", sent.RootElement.GetProperty("workload_identity_pool_provider_id").GetString());
        Assert.False(sent.RootElement.GetProperty("use_inherited_settings").GetBoolean());
    }

    [Fact]
    public async Task SetGooglePlayAsync_PutsToTheGooglePlaySlugRoute()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetGooglePlayAsync("group/project",
            new GooglePlayIntegrationRequest
            {
                PackageName = "com.example.app",
                ServiceAccountKeyFileName = "key.json",
                ServiceAccountKey = "{\"type\":\"service_account\"}",
                GooglePlayProtectedRefs = true
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fproject/integrations/google-play",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("com.example.app", sent.RootElement.GetProperty("package_name").GetString());
        Assert.Equal("key.json", sent.RootElement.GetProperty("service_account_key_file_name").GetString());
        Assert.True(sent.RootElement.GetProperty("google_play_protected_refs").GetBoolean());
    }

    [Fact]
    public async Task SetHangoutsChatAsync_SerializesTheWebhookAsAUri()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetHangoutsChatAsync(1,
            new HangoutsChatIntegrationRequest
            {
                Webhook = new Uri("https://chat.googleapis.com/v1/spaces/AAA/messages"),
                NotifyOnlyBrokenPipelines = true,
                BranchesToBeNotified = "default"
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/hangouts-chat",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("https://chat.googleapis.com/v1/spaces/AAA/messages",
            sent.RootElement.GetProperty("webhook").GetString());
        Assert.Equal("default", sent.RootElement.GetProperty("branches_to_be_notified").GetString());
        Assert.True(sent.RootElement.GetProperty("notify_only_broken_pipelines").GetBoolean());
    }

    [Fact]
    public async Task SetHarborAsync_PutsAllFourRequiredCredentialFields()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetHarborAsync(1,
            new HarborIntegrationRequest
            {
                Url = new Uri("https://demo.goharbor.io"),
                ProjectName = "testproject",
                Username = "admin",
                Password = "s3cr3t"
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/harbor",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("https://demo.goharbor.io", sent.RootElement.GetProperty("url").GetString());
        Assert.Equal("testproject", sent.RootElement.GetProperty("project_name").GetString());
        Assert.Equal("admin", sent.RootElement.GetProperty("username").GetString());
        Assert.Equal("s3cr3t", sent.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task SetIrkerAsync_OmitsOptionalMembersWhenNull()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetIrkerAsync(1, new IrkerIntegrationRequest { Recipients = "#gitlab, dev@example.com" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/irker",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("#gitlab, dev@example.com", sent.RootElement.GetProperty("recipients").GetString());
        Assert.Single(sent.RootElement.EnumerateObject());
    }

    [Fact]
    public async Task SetJenkinsAsync_PutsTheJenkinsUrlAndProjectName()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetJenkinsAsync(1,
            new JenkinsIntegrationRequest
            {
                JenkinsUrl = new Uri("http://jenkins.example.com/"),
                ProjectName = "my_project_name",
                EnableSslVerification = true,
                MergeRequestsEvents = true
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jenkins",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("http://jenkins.example.com/", sent.RootElement.GetProperty("jenkins_url").GetString());
        Assert.Equal("my_project_name", sent.RootElement.GetProperty("project_name").GetString());
        Assert.True(sent.RootElement.GetProperty("enable_ssl_verification").GetBoolean());
        Assert.True(sent.RootElement.GetProperty("merge_requests_events").GetBoolean());
    }

    [Fact]
    public async Task SetJiraAsync_SendsUrlAndPasswordAndTheOddlyTypedIssuesEnabledString()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetJiraAsync(1,
            new JiraIntegrationRequest
            {
                Url = new Uri("https://jira.example.com"),
                Password = "api-token",
                Username = "bot@example.com",
                IssuesEnabled = "true",
                ProjectKeys = ["PROJ", "OTHER"]
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jira",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("https://jira.example.com", sent.RootElement.GetProperty("url").GetString());
        Assert.Equal("api-token", sent.RootElement.GetProperty("password").GetString());
        Assert.Equal("bot@example.com", sent.RootElement.GetProperty("username").GetString());

        // Spec-typed as a string on this endpoint, not a bool - verified by GetString() succeeding.
        Assert.Equal("true", sent.RootElement.GetProperty("issues_enabled").GetString());

        Assert.Equal(2, sent.RootElement.GetProperty("project_keys").GetArrayLength());
    }

    [Fact]
    public async Task SetJiraAsync_UsesTheExplicitWireNameForVulnerabilitiesIssueType()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetJiraAsync(1,
            new JiraIntegrationRequest
            {
                Url = new Uri("https://jira.example.com"),
                Password = "api-token",
                VulnerabilitiesEnabled = true,
                VulnerabilitiesIssueType = "10004"
            }, TestContext.Current.CancellationToken);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());

        // The C# member is VulnerabilitiesIssueType (idiomatic casing); GitLab's wire name has no
        // internal capital to split on, so it needs the explicit [JsonPropertyName] override.
        Assert.Equal("10004", sent.RootElement.GetProperty("vulnerabilities_issuetype").GetString());
        Assert.False(sent.RootElement.TryGetProperty("vulnerabilities_issue_type", out _));
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jira",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetJiraCloudAppAsync_AllowsAnEntirelyEmptyBody()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetJiraCloudAppAsync(1, new JiraCloudAppIntegrationRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jira-cloud-app",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody());
    }

    [Fact]
    public async Task SetJiraCloudAppAsync_SendsTheServiceIdsAndGatingFields()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetJiraCloudAppAsync(1,
            new JiraCloudAppIntegrationRequest
            {
                JiraCloudAppServiceIds = "1001,1002",
                JiraCloudAppEnableDeploymentGating = true,
                JiraCloudAppDeploymentGatingEnvironments = "production,staging"
            }, TestContext.Current.CancellationToken);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("1001,1002", sent.RootElement.GetProperty("jira_cloud_app_service_ids").GetString());
        Assert.True(sent.RootElement.GetProperty("jira_cloud_app_enable_deployment_gating").GetBoolean());
        Assert.Equal("production,staging",
            sent.RootElement.GetProperty("jira_cloud_app_deployment_gating_environments").GetString());
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/jira-cloud-app",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetLinearAsync_PutsTheWorkspaceUrl()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetLinearAsync(1,
            new LinearIntegrationRequest { WorkspaceUrl = new Uri("https://linear.app/example") },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/linear",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("https://linear.app/example", sent.RootElement.GetProperty("workspace_url").GetString());
    }

    [Fact]
    public async Task SetMatrixAsync_PutsTheTokenAndRoom()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        await repository.SetMatrixAsync(1,
            new MatrixIntegrationRequest
            {
                Token = "syt-zyx57W2v1u123ew11",
                Room = "!qPKKM111FFKKsfoCVy:matrix.org",
                NotifyOnlyBrokenPipelines = true
            }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/matrix",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("syt-zyx57W2v1u123ew11", sent.RootElement.GetProperty("token").GetString());
        Assert.Equal("!qPKKM111FFKKsfoCVy:matrix.org", sent.RootElement.GetProperty("room").GetString());
        Assert.False(sent.RootElement.TryGetProperty("hostname", out _));
    }

    [Fact]
    public async Task SetMattermostAsync_PutsTheWebhookAndPerEventChannelOverrides()
    {
        (IntegrationsRepository repository, StubHttpMessageHandler handler, Func<string?> sentBody) =
            CreateRepository();

        GitLabIntegration integration = await repository.SetMattermostAsync(1,
            new MattermostIntegrationRequest
            {
                Webhook = new Uri("http://mattermost.example.com/hooks/xyz"),
                Channel = "general",
                PushChannel = "push-notifications",
                MergeRequestChannel = "merge-requests",
                LabelsToBeNotifiedBehavior = "match_all"
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/integrations/mattermost",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using JsonDocument sent = JsonDocument.Parse(sentBody() ?? throw new InvalidOperationException());
        Assert.Equal("http://mattermost.example.com/hooks/xyz", sent.RootElement.GetProperty("webhook").GetString());
        Assert.Equal("general", sent.RootElement.GetProperty("channel").GetString());
        Assert.Equal("push-notifications", sent.RootElement.GetProperty("push_channel").GetString());
        Assert.Equal("merge-requests", sent.RootElement.GetProperty("merge_request_channel").GetString());
        Assert.Equal("match_all", sent.RootElement.GetProperty("labels_to_be_notified_behavior").GetString());

        Assert.Equal(42, integration.Id);
        Assert.Equal("some-integration", integration.Slug);
    }
}