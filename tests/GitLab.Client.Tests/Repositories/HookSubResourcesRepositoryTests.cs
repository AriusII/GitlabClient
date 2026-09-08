using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Covers the sub-resources the three hook surfaces share - the test-trigger endpoints, the delivery
///     log and its resend, and the URL-variable / custom-header keys - where the interesting behaviour is
///     route construction and encoding rather than payload mapping.
/// </summary>
public sealed class HookSubResourcesRepositoryTests
{
    private const string EventsJson = """
                                      [
                                        {
                                          "id": 981,
                                          "url": "https://example.com/hook/{secret_token}",
                                          "trigger": "push_hooks",
                                          "request_headers": {
                                            "Content-Type": "application/json",
                                            "X-Gitlab-Event": "Push Hook"
                                          },
                                          "request_data": {
                                            "object_kind": "push",
                                            "total_commits_count": 2
                                          },
                                          "response_headers": { "Content-Type": "text/plain" },
                                          "response_body": "ok",
                                          "response_status": "200",
                                          "execution_duration": 0.34,
                                          "internal_error_message": null
                                        }
                                      ]
                                      """;

    [Fact]
    public async Task ProjectTestAsync_PostsToTestRoute_WithTheTriggerWireName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksRepository repository = new(connection);

        await repository.TestAsync(42, 7, GitLabWebhookTestTrigger.MergeRequestsEvents,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/test/merge_requests_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task GroupTestAsync_PostsToTestRoute_WithNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksRepository repository = new(connection);

        await repository.TestAsync("gitlab-org/subgroup", 3, GitLabWebhookTestTrigger.ConfidentialNoteEvents,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/hooks/3/test/confidential_note_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Theory]
    [InlineData(GitLabWebhookTestTrigger.PushEvents, "push_events")]
    [InlineData(GitLabWebhookTestTrigger.IssuesEvents, "issues_events")]
    [InlineData(GitLabWebhookTestTrigger.ConfidentialIssuesEvents, "confidential_issues_events")]
    [InlineData(GitLabWebhookTestTrigger.MergeRequestsEvents, "merge_requests_events")]
    [InlineData(GitLabWebhookTestTrigger.NoteEvents, "note_events")]
    [InlineData(GitLabWebhookTestTrigger.ConfidentialNoteEvents, "confidential_note_events")]
    [InlineData(GitLabWebhookTestTrigger.JobEvents, "job_events")]
    [InlineData(GitLabWebhookTestTrigger.PipelineEvents, "pipeline_events")]
    [InlineData(GitLabWebhookTestTrigger.DeploymentEvents, "deployment_events")]
    [InlineData(GitLabWebhookTestTrigger.FeatureFlagEvents, "feature_flag_events")]
    [InlineData(GitLabWebhookTestTrigger.MilestoneEvents, "milestone_events")]
    [InlineData(GitLabWebhookTestTrigger.EmojiEvents, "emoji_events")]
    public async Task TestAsync_MapsEveryTrigger_ToItsSpecWireName(GitLabWebhookTestTrigger trigger, string expected)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksRepository repository = new(connection);

        await repository.TestAsync(1, 1, trigger, TestContext.Current.CancellationToken);

        Assert.Equal($"https://gitlab.example/api/v4/projects/1/hooks/1/test/{expected}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListEventsAsync_BuildsEventsRoute_AndDeserializesTheDeliveryLog()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(EventsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksRepository repository = new(connection);

        List<GitLabHookEvent> events = new();
        await foreach (GitLabHookEvent item in repository.ListEventsAsync(42, 7,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabHookEvent entry = Assert.Single(events);
        Assert.Equal(981, entry.Id);

        // GitLab masks a URL variable back to its {name} placeholder in the log, so the recorded URL is not
        // a plain absolute URI - assert it survives round-tripping rather than assuming it does.
        Assert.Equal("https://example.com/hook/%7Bsecret_token%7D", entry.Url?.AbsoluteUri);
        Assert.Equal("push_hooks", entry.Trigger);
        Assert.Equal("200", entry.ResponseStatus);
        Assert.Equal(0.34, entry.ExecutionDuration);
        Assert.Equal("ok", entry.ResponseBody);
        Assert.Null(entry.InternalErrorMessage);
        Assert.Equal("Push Hook", entry.RequestHeaders?["X-Gitlab-Event"]);
        Assert.Equal("text/plain", entry.ResponseHeaders?["Content-Type"]);
        Assert.Equal("push", entry.RequestData?.GetProperty("object_kind").GetString());
        Assert.Equal(2, entry.RequestData?.GetProperty("total_commits_count").GetInt32());
    }

    [Fact]
    public async Task ListEventsAsync_ProjectsTheStatusFilter_AsACommaJoinedQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksRepository repository = new(connection);

        HookEventListOptions options = new() { Status = ["500", "server_failure"], PerPage = 20 };

        await foreach (GitLabHookEvent _ in repository.ListEventsAsync(9, 4, options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/4/events?status=500,server_failure&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ResendEventAsync_PostsToTheHookLogResendRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksRepository repository = new(connection);

        await repository.ResendEventAsync("gitlab-org/gitlab", 7, 981, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/hooks/7/events/981/resend",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GroupResendEventAsync_PostsToTheHookLogResendRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksRepository repository = new(connection);

        await repository.ResendEventAsync(9, 4, 12, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/4/events/12/resend",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteUrlVariableAsync_PercentEncodesTheKey_OnEveryHookSurface()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);

        await new ProjectHooksRepository(connection).DeleteUrlVariableAsync(42, 7, "token/1",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/url_variables/token%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new GroupHooksRepository(connection).DeleteUrlVariableAsync(9, 4, "token/1",
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/4/url_variables/token%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new SystemHooksRepository(connection).DeleteUrlVariableAsync(3, "token/1",
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/url_variables/token%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteCustomHeaderAsync_PercentEncodesTheKey_OnEveryHookSurface()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);

        await new ProjectHooksRepository(connection).DeleteCustomHeaderAsync(42, 7, "X-Team Header",
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/custom_headers/X-Team%20Header",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new GroupHooksRepository(connection).DeleteCustomHeaderAsync("gitlab-org/subgroup", 4, "X-Team Header",
            TestContext.Current.CancellationToken);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/hooks/4/custom_headers/X-Team%20Header",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new SystemHooksRepository(connection).DeleteCustomHeaderAsync(3, "X-Team Header",
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/custom_headers/X-Team%20Header",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateUrlVariableAsync_PutsTheValue_WithNoResponseBodyExpected_OnEveryHookSurface()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);

        await new ProjectHooksRepository(connection).UpdateUrlVariableAsync(42, 7, "token/1",
            new UpdateProjectHookUrlVariableRequest { Value = "s3cr3t" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/url_variables/token%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new GroupHooksRepository(connection).UpdateUrlVariableAsync(9, 4, "token/1",
            new UpdateGroupHookUrlVariableRequest { Value = "s3cr3t" }, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/4/url_variables/token%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateCustomHeaderAsync_PutsTheValue_WithNoResponseBodyExpected_OnEveryHookSurface()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);

        await new ProjectHooksRepository(connection).UpdateCustomHeaderAsync(42, 7, "X-Team Header",
            new UpdateProjectHookCustomHeaderRequest { Value = "secret-value" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/custom_headers/X-Team%20Header",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await new GroupHooksRepository(connection).UpdateCustomHeaderAsync("gitlab-org/subgroup", 4, "X-Team Header",
            new UpdateGroupHookCustomHeaderRequest { Value = "secret-value" },
            TestContext.Current.CancellationToken);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/hooks/4/custom_headers/X-Team%20Header",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public void UpdateGroupHookCustomHeaderRequest_ToString_RedactsTheValue()
    {
        UpdateGroupHookCustomHeaderRequest request = new() { Value = "super-secret" };

        string rendered = request.ToString();

        Assert.DoesNotContain("super-secret", rendered, StringComparison.Ordinal);
        Assert.Contains("redacted", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdateProjectHookUrlVariableRequest_ToString_RedactsTheValue()
    {
        UpdateProjectHookUrlVariableRequest request = new() { Value = "super-secret" };

        string rendered = request.ToString();

        Assert.DoesNotContain("super-secret", rendered, StringComparison.Ordinal);
        Assert.Contains("redacted", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void HookEventRequestData_StaysRawJson_SoAnyTriggerPayloadRoundTrips()
    {
        using JsonDocument document = JsonDocument.Parse("""{ "object_kind": "merge_request" }""");

        GitLabHookEvent entry = new() { Id = 1, RequestData = document.RootElement.Clone() };

        Assert.Equal(JsonValueKind.Object, entry.RequestData?.ValueKind);
    }
}