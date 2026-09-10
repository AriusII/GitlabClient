using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ProjectHooksEndpointTests
{
    [Fact]
    public async Task ListAsync_WithOptions_UsesTheProjectHookPaginationQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);
        ProjectHookListOptions options = new() { Page = 3, PerPage = 50 };

        await foreach (GitLabProjectHook _ in repository.ListAsync(42, options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks?page=3&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_BuildsHooksRoute_AndDeserializesAllPages()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "url": "https://example.com/hook",
                                "push_events": true,
                                "issues_events": false,
                                "merge_requests_events": false,
                                "tag_push_events": false,
                                "note_events": false,
                                "pipeline_events": false,
                                "enable_ssl_verification": true,
                                "created_at": "2024-05-01T10:00:00.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        List<GitLabProjectHook> hooks = new();
        await foreach (GitLabProjectHook item in repository.ListAsync(42, TestContext.Current.CancellationToken))
        {
            hooks.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabProjectHook hook = Assert.Single(hooks);
        Assert.Equal(1, hook.Id);
        Assert.Equal(new Uri("https://example.com/hook"), hook.Url);
        Assert.True(hook.PushEvents);
        Assert.False(hook.IssuesEvents);
        Assert.True(hook.EnableSslVerification);
        Assert.Equal(DateTimeOffset.Parse("2024-05-01T10:00:00.000Z", CultureInfo.InvariantCulture), hook.CreatedAt);
    }

    [Fact]
    public async Task GetAsync_BuildsHookRoute_WithNamespacedProjectPath_AndDeserializesHook()
    {
        const string Json = """
                            {
                              "id": 7,
                              "url": "https://example.com/webhook",
                              "push_events": true,
                              "issues_events": true,
                              "merge_requests_events": false,
                              "tag_push_events": false,
                              "note_events": false,
                              "pipeline_events": true,
                              "enable_ssl_verification": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        GitLabProjectHook hook =
            await repository.GetAsync("gitlab-org/gitlab", 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/hooks/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, hook.Id);
        Assert.True(hook.PipelineEvents);
        Assert.False(hook.EnableSslVerification);
        Assert.Null(hook.CreatedAt);
    }

    [Fact]
    public async Task AddAsync_PostsToHooksRoute_WithSerializedBody_AndDeserializesCreatedHook()
    {
        const string Json = """
                            {
                              "id": 99,
                              "url": "https://example.com/new-hook",
                              "push_events": true,
                              "issues_events": false,
                              "merge_requests_events": true,
                              "tag_push_events": false,
                              "note_events": false,
                              "pipeline_events": false,
                              "enable_ssl_verification": true
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        CreateProjectHookRequest request = new()
        {
            Url = new Uri("https://example.com/new-hook"), PushEvents = true, MergeRequestsEvents = true
        };

        GitLabProjectHook hook = await repository.AddAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"url\":\"https://example.com/new-hook\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"push_events\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal(99, hook.Id);
        Assert.Equal(new Uri("https://example.com/new-hook"), hook.Url);
        Assert.True(hook.MergeRequestsEvents);
    }

    [Fact]
    public async Task UpdateAsync_PutsToHookRoute_WithSerializedBody_AndDeserializesUpdatedHook()
    {
        const string Json = """
                            {
                              "id": 7,
                              "url": "https://example.com/updated-hook",
                              "push_events": false,
                              "issues_events": true,
                              "merge_requests_events": false,
                              "tag_push_events": false,
                              "note_events": true,
                              "pipeline_events": false,
                              "enable_ssl_verification": true
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        UpdateProjectHookRequest request = new()
        {
            Url = new Uri("https://example.com/updated-hook"), NoteEvents = true
        };

        GitLabProjectHook hook = await repository.UpdateAsync(42, 7, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"url\":\"https://example.com/updated-hook\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"note_events\":true", sentBody, StringComparison.Ordinal);
        Assert.True(hook.NoteEvents);
        Assert.True(hook.IssuesEvents);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToHookRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        await repository.DeleteAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Project Hook Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Hook Not Found", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Project Hook Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteAsync(42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Hook Not Found", exception.Message);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheFullHookEntity_IncludingKeyOnlyVariablesAndHeaders()
    {
        const string Json = """
                            {
                              "id": 7,
                              "url": "https://example.com/webhook",
                              "name": "Deploy notifier",
                              "description": "Pings the deploy channel",
                              "project_id": 42,
                              "organization_id": 1,
                              "created_at": "2024-05-01T10:00:00.000Z",
                              "push_events": true,
                              "push_events_branch_filter": "main",
                              "branch_filter_strategy": "all_branches",
                              "wiki_page_events": true,
                              "deployment_events": true,
                              "feature_flag_events": true,
                              "releases_events": true,
                              "milestone_events": true,
                              "emoji_events": true,
                              "resource_access_token_events": true,
                              "resource_deploy_token_events": true,
                              "vulnerability_events": true,
                              "duo_flow_callback_enabled": true,
                              "enable_ssl_verification": false,
                              "alert_status": "temporarily_disabled",
                              "disabled_until": "2024-05-02T10:00:00.000Z",
                              "token_present": true,
                              "signing_token_present": false,
                              "custom_webhook_template": "{\"text\":\"{{object_kind}}\"}",
                              "url_variables": [ { "key": "deploy_token" } ],
                              "custom_headers": [ { "key": "X-Deploy" } ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        GitLabProjectHook hook = await repository.GetAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal("Deploy notifier", hook.Name);
        Assert.Equal(42, hook.ProjectId);
        Assert.Equal(1, hook.OrganizationId);
        Assert.Equal("main", hook.PushEventsBranchFilter);
        Assert.Equal("all_branches", hook.BranchFilterStrategy);
        Assert.True(hook.WikiPageEvents);
        Assert.True(hook.ResourceDeployTokenEvents);
        Assert.True(hook.DuoFlowCallbackEnabled);
        Assert.Equal("temporarily_disabled", hook.AlertStatus);
        Assert.Equal(DateTimeOffset.Parse("2024-05-02T10:00:00.000Z", CultureInfo.InvariantCulture),
            hook.DisabledUntil);

        // The token itself is never on the wire - only the fact that one is configured.
        Assert.True(hook.TokenPresent);
        Assert.False(hook.SigningTokenPresent);
        Assert.Equal("deploy_token", Assert.Single(hook.UrlVariables ?? []).Key);
        Assert.Null(Assert.Single(hook.UrlVariables ?? []).Value);
        Assert.Equal("X-Deploy", Assert.Single(hook.CustomHeaders ?? []).Key);
    }

    [Fact]
    public async Task AddAsync_SerializesTheSecretsVariablesAndBranchFilterStrategy()
    {
        const string Json = """{ "id": 99, "url": "https://example.com/new-hook" }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        CreateProjectHookRequest request = new()
        {
            Url = new Uri("https://example.com/new-hook/{deploy_token}"),
            Token = "s3cret",
            SigningToken = "whsec_abc",
            BranchFilterStrategy = GitLabHookBranchFilterStrategy.Wildcard,
            PushEventsBranchFilter = "release/*",
            UrlVariables = [new GitLabHookUrlVariable { Key = "deploy_token", Value = "abc" }],
            CustomHeaders = [new GitLabHookCustomHeader { Key = "X-Deploy", Value = "yes" }]
        };

        await repository.AddAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Contains("\"branch_filter_strategy\":\"wildcard\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"push_events_branch_filter\":\"release/*\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"token\":\"s3cret\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"signing_token\":\"whsec_abc\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"url_variables\":[{\"key\":\"deploy_token\",\"value\":\"abc\"}]", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"custom_headers\":[{\"key\":\"X-Deploy\",\"value\":\"yes\"}]", sentBody,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_WithoutAUrl_SendsOnlyTheChangedFlags()
    {
        const string Json = """{ "id": 7, "url": "https://example.com/webhook" }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectHooksClient repository = new(connection);

        UpdateProjectHookRequest request = new() { EmojiEvents = true };

        await repository.UpdateAsync(42, 7, request, TestContext.Current.CancellationToken);

        Assert.Equal("""{"emoji_events":true}""", sentBody);
    }
}