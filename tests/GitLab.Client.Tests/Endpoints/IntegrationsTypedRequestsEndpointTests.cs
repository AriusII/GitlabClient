using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Verifies the final twenty typed integration setters. Each row creates every documented member of
///     one request record, proving the fixed modern route and the complete GitLab 19.4 wire shape.
/// </summary>
public sealed class IntegrationsTypedRequestsEndpointTests
{
    private const string ResponseJson = """
                                        {
                                          "id": 42,
                                          "title": "Some integration",
                                          "slug": "some-integration",
                                          "active": true
                                        }
                                        """;

    public static TheoryData<string> TypedIntegrationSlugs =>
    [
        GitLabIntegrationSlug.AppleAppStore, GitLabIntegrationSlug.Asana, GitLabIntegrationSlug.Assembla,
        GitLabIntegrationSlug.Bamboo, GitLabIntegrationSlug.Bugzilla, GitLabIntegrationSlug.Buildkite,
        GitLabIntegrationSlug.Campfire, GitLabIntegrationSlug.ClickUp, GitLabIntegrationSlug.MattermostSlashCommands,
        GitLabIntegrationSlug.MicrosoftTeams, GitLabIntegrationSlug.MockCi, GitLabIntegrationSlug.MockMonitoring,
        GitLabIntegrationSlug.Packagist, GitLabIntegrationSlug.Phorge, GitLabIntegrationSlug.PipelinesEmail,
        GitLabIntegrationSlug.PivotalTracker, GitLabIntegrationSlug.Pumble, GitLabIntegrationSlug.Pushover,
        GitLabIntegrationSlug.Redmine, GitLabIntegrationSlug.Slack
    ];

    [Theory]
    [MemberData(nameof(TypedIntegrationSlugs))]
    public async Task SetTypedIntegrationAsync_UsesTheExpectedPutRouteAndCompleteRequestBody(string slug)
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsClient client = new(connection);

        GitLabIntegration integration = await SetAsync(client, slug, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/1/integrations/{slug}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(ExpectedRequestBody(slug), sentBody);
        Assert.Equal(42, integration.Id);
    }

    private static Task<GitLabIntegration> SetAsync(IntegrationsClient client, string slug, CancellationToken ct)
    {
        return slug switch
        {
            GitLabIntegrationSlug.AppleAppStore => client.SetAppleAppStoreAsync(1,
                new AppleAppStoreIntegrationRequest
                {
                    AppStoreIssuerId = "issuer-1",
                    AppStoreKeyId = "key-1",
                    AppStorePrivateKeyFileName = "AuthKey_key-1.p8",
                    AppStorePrivateKey = "private-key",
                    AppStoreProtectedRefs = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Asana => client.SetAsanaAsync(1,
                new AsanaIntegrationRequest
                {
                    ApiKey = "asana-token",
                    RestrictToBranch = "main,release",
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Assembla => client.SetAssemblaAsync(1,
                new AssemblaIntegrationRequest
                {
                    Token = "assembla-token", Subdomain = "acme", PushEvents = true, UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Bamboo => client.SetBambooAsync(1,
                new BambooIntegrationRequest
                {
                    EnableSslVerification = true,
                    BambooUrl = new Uri("https://bamboo.example.test/build"),
                    BuildKey = "ACME-PLAN",
                    Username = "builder",
                    Password = "password",
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Bugzilla => client.SetBugzillaAsync(1,
                new BugzillaIntegrationRequest
                {
                    ProjectUrl = new Uri("https://bugs.example.test/projects/acme"),
                    IssuesUrl = new Uri("https://bugs.example.test/issues/42"),
                    NewIssueUrl = new Uri("https://bugs.example.test/issues/new"),
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Buildkite => client.SetBuildkiteAsync(1,
                new BuildkiteIntegrationRequest
                {
                    ProjectUrl = new Uri("https://buildkite.example.test/pipelines/acme"),
                    Token = "buildkite-token",
                    EnableSslVerification = true,
                    PushEvents = true,
                    MergeRequestsEvents = true,
                    TagPushEvents = false,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Campfire => client.SetCampfireAsync(1,
                new CampfireIntegrationRequest
                {
                    Token = "campfire-token",
                    Subdomain = "acme",
                    Room = "engineering",
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.ClickUp => client.SetClickUpAsync(1,
                new ClickUpIntegrationRequest
                {
                    ProjectUrl = new Uri("https://app.clickup.com/team/acme"),
                    IssuesUrl = new Uri("https://app.clickup.com/t/42"),
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.MattermostSlashCommands => client.SetMattermostSlashCommandsAsync(1,
                new MattermostSlashCommandsIntegrationRequest
                {
                    Token = "mattermost-token", UseInheritedSettings = false
                },
                ct),
            GitLabIntegrationSlug.MicrosoftTeams => client.SetMicrosoftTeamsAsync(1,
                new MicrosoftTeamsIntegrationRequest
                {
                    Webhook = new Uri("https://teams.example.test/hooks/acme"),
                    NotifyOnlyBrokenPipelines = true,
                    NotifyOnlyWhenPipelineStatusChanges = true,
                    BranchesToBeNotified = "default",
                    PushEvents = true,
                    IssuesEvents = true,
                    ConfidentialIssuesEvents = false,
                    WorkItemEvents = true,
                    ConfidentialWorkItemEvents = false,
                    MergeRequestsEvents = true,
                    NoteEvents = true,
                    ConfidentialNoteEvents = false,
                    TagPushEvents = true,
                    PipelineEvents = true,
                    WikiPageEvents = false,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.MockCi => client.SetMockCiAsync(1,
                new MockCiIntegrationRequest
                {
                    EnableSslVerification = true,
                    MockServiceUrl = new Uri("https://mock-ci.example.test/api"),
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.MockMonitoring => client.SetMockMonitoringAsync(1,
                new MockMonitoringIntegrationRequest { UseInheritedSettings = false }, ct),
            GitLabIntegrationSlug.Packagist => client.SetPackagistAsync(1,
                new PackagistIntegrationRequest
                {
                    Username = "packagist-user",
                    Token = "packagist-token",
                    Server = "https://packagist.example.test",
                    PushEvents = true,
                    MergeRequestsEvents = true,
                    TagPushEvents = false,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Phorge => client.SetPhorgeAsync(1,
                new PhorgeIntegrationRequest
                {
                    ProjectUrl = new Uri("https://phorge.example.test/projects/acme"),
                    IssuesUrl = new Uri("https://phorge.example.test/T42"),
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.PipelinesEmail => client.SetPipelinesEmailAsync(1,
                new PipelinesEmailIntegrationRequest
                {
                    Recipients = "dev@example.test ops@example.test",
                    NotifyOnlyBrokenPipelines = true,
                    NotifyOnlyWhenPipelineStatusChanges = true,
                    NotifyOnlyDefaultBranch = false,
                    BranchesToBeNotified = "default",
                    NotifyChildPipelines = true,
                    PipelineEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.PivotalTracker => client.SetPivotalTrackerAsync(1,
                new PivotalTrackerIntegrationRequest
                {
                    Token = "pivotal-token",
                    RestrictToBranch = "main,release",
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Pumble => client.SetPumbleAsync(1,
                new PumbleIntegrationRequest
                {
                    Webhook = new Uri("https://pumble.example.test/hooks/acme"),
                    NotifyOnlyBrokenPipelines = true,
                    NotifyOnlyWhenPipelineStatusChanges = true,
                    BranchesToBeNotified = "default",
                    PushEvents = true,
                    IssuesEvents = true,
                    ConfidentialIssuesEvents = false,
                    WorkItemEvents = true,
                    ConfidentialWorkItemEvents = false,
                    MergeRequestsEvents = true,
                    NoteEvents = true,
                    ConfidentialNoteEvents = false,
                    TagPushEvents = true,
                    PipelineEvents = true,
                    WikiPageEvents = false,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Pushover => client.SetPushoverAsync(1,
                new PushoverIntegrationRequest
                {
                    ApiKey = "pushover-api-key",
                    UserKey = "pushover-user-key",
                    Device = "phone",
                    Priority = "1",
                    Sound = "persistent",
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Redmine => client.SetRedmineAsync(1,
                new RedmineIntegrationRequest
                {
                    ProjectUrl = new Uri("https://redmine.example.test/projects/acme"),
                    IssuesUrl = new Uri("https://redmine.example.test/issues/42"),
                    NewIssueUrl = new Uri("https://redmine.example.test/issues/new"),
                    PushEvents = true,
                    UseInheritedSettings = false
                }, ct),
            GitLabIntegrationSlug.Slack => client.SetSlackAsync(1,
                new SlackIntegrationRequest
                {
                    Webhook = new Uri("https://hooks.slack.example.test/services/acme"),
                    Username = "GitLab",
                    Channel = "#general",
                    NotifyOnlyBrokenPipelines = true,
                    NotifyOnlyWhenPipelineStatusChanges = true,
                    BranchesToBeNotified = "default",
                    LabelsToBeNotified = "bug,urgent",
                    LabelsToBeNotifiedBehavior = "match_any",
                    PushChannel = "#push",
                    IssueChannel = "#issues",
                    IncidentChannel = "#incidents",
                    AlertChannel = "#alerts",
                    ConfidentialIssueChannel = "#private-issues",
                    MergeRequestChannel = "#merge-requests",
                    NoteChannel = "#notes",
                    ConfidentialNoteChannel = "#private-notes",
                    TagPushChannel = "#tags",
                    DeploymentChannel = "#deployments",
                    PipelineChannel = "#pipelines",
                    WikiPageChannel = "#wiki",
                    VulnerabilityChannel = "#vulnerabilities",
                    PushEvents = true,
                    IssuesEvents = true,
                    ConfidentialIssuesEvents = false,
                    MergeRequestsEvents = true,
                    NoteEvents = true,
                    ConfidentialNoteEvents = false,
                    TagPushEvents = true,
                    PipelineEvents = true,
                    WikiPageEvents = false,
                    DeploymentEvents = true,
                    IncidentEvents = true,
                    WorkItemEvents = true,
                    ConfidentialWorkItemEvents = false,
                    VulnerabilityEvents = true,
                    AlertEvents = true,
                    UseInheritedSettings = false
                }, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(slug), slug, "Unsupported integration slug.")
        };
    }

    private static string ExpectedRequestBody(string slug)
    {
        return slug switch
        {
            GitLabIntegrationSlug.AppleAppStore =>
                """{"app_store_issuer_id":"issuer-1","app_store_key_id":"key-1","app_store_private_key_file_name":"AuthKey_key-1.p8","app_store_private_key":"private-key","app_store_protected_refs":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Asana =>
                """{"api_key":"asana-token","restrict_to_branch":"main,release","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Assembla =>
                """{"token":"assembla-token","subdomain":"acme","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Bamboo =>
                """{"enable_ssl_verification":true,"bamboo_url":"https://bamboo.example.test/build","build_key":"ACME-PLAN","username":"builder","password":"password","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Bugzilla =>
                """{"project_url":"https://bugs.example.test/projects/acme","issues_url":"https://bugs.example.test/issues/42","new_issue_url":"https://bugs.example.test/issues/new","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Buildkite =>
                """{"project_url":"https://buildkite.example.test/pipelines/acme","token":"buildkite-token","enable_ssl_verification":true,"push_events":true,"merge_requests_events":true,"tag_push_events":false,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Campfire =>
                """{"token":"campfire-token","subdomain":"acme","room":"engineering","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.ClickUp =>
                """{"project_url":"https://app.clickup.com/team/acme","issues_url":"https://app.clickup.com/t/42","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.MattermostSlashCommands =>
                """{"token":"mattermost-token","use_inherited_settings":false}""",
            GitLabIntegrationSlug.MicrosoftTeams =>
                """{"webhook":"https://teams.example.test/hooks/acme","notify_only_broken_pipelines":true,"notify_only_when_pipeline_status_changes":true,"branches_to_be_notified":"default","push_events":true,"issues_events":true,"confidential_issues_events":false,"work_item_events":true,"confidential_work_item_events":false,"merge_requests_events":true,"note_events":true,"confidential_note_events":false,"tag_push_events":true,"pipeline_events":true,"wiki_page_events":false,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.MockCi =>
                """{"enable_ssl_verification":true,"mock_service_url":"https://mock-ci.example.test/api","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.MockMonitoring => """{"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Packagist =>
                """{"username":"packagist-user","token":"packagist-token","server":"https://packagist.example.test","push_events":true,"merge_requests_events":true,"tag_push_events":false,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Phorge =>
                """{"project_url":"https://phorge.example.test/projects/acme","issues_url":"https://phorge.example.test/T42","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.PipelinesEmail =>
                """{"recipients":"dev@example.test ops@example.test","notify_only_broken_pipelines":true,"notify_only_when_pipeline_status_changes":true,"notify_only_default_branch":false,"branches_to_be_notified":"default","notify_child_pipelines":true,"pipeline_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.PivotalTracker =>
                """{"token":"pivotal-token","restrict_to_branch":"main,release","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Pumble =>
                """{"webhook":"https://pumble.example.test/hooks/acme","notify_only_broken_pipelines":true,"notify_only_when_pipeline_status_changes":true,"branches_to_be_notified":"default","push_events":true,"issues_events":true,"confidential_issues_events":false,"work_item_events":true,"confidential_work_item_events":false,"merge_requests_events":true,"note_events":true,"confidential_note_events":false,"tag_push_events":true,"pipeline_events":true,"wiki_page_events":false,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Pushover =>
                """{"api_key":"pushover-api-key","user_key":"pushover-user-key","device":"phone","priority":"1","sound":"persistent","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Redmine =>
                """{"project_url":"https://redmine.example.test/projects/acme","issues_url":"https://redmine.example.test/issues/42","new_issue_url":"https://redmine.example.test/issues/new","push_events":true,"use_inherited_settings":false}""",
            GitLabIntegrationSlug.Slack =>
                """{"webhook":"https://hooks.slack.example.test/services/acme","username":"GitLab","channel":"#general","notify_only_broken_pipelines":true,"notify_only_when_pipeline_status_changes":true,"branches_to_be_notified":"default","labels_to_be_notified":"bug,urgent","labels_to_be_notified_behavior":"match_any","push_channel":"#push","issue_channel":"#issues","incident_channel":"#incidents","alert_channel":"#alerts","confidential_issue_channel":"#private-issues","merge_request_channel":"#merge-requests","note_channel":"#notes","confidential_note_channel":"#private-notes","tag_push_channel":"#tags","deployment_channel":"#deployments","pipeline_channel":"#pipelines","wiki_page_channel":"#wiki","vulnerability_channel":"#vulnerabilities","push_events":true,"issues_events":true,"confidential_issues_events":false,"merge_requests_events":true,"note_events":true,"confidential_note_events":false,"tag_push_events":true,"pipeline_events":true,"wiki_page_events":false,"deployment_events":true,"incident_events":true,"work_item_events":true,"confidential_work_item_events":false,"vulnerability_events":true,"alert_events":true,"use_inherited_settings":false}""",
            _ => throw new ArgumentOutOfRangeException(nameof(slug), slug, "Unsupported integration slug.")
        };
    }
}