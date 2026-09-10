using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class GroupHooksEndpointTests
{
    private const string HookJson = """
                                    {
                                      "id": 3,
                                      "url": "https://example.com/group-hook",
                                      "name": "Release notifier",
                                      "description": "Posts to the release channel",
                                      "group_id": 9,
                                      "created_at": "2024-05-01T10:00:00.000Z",
                                      "push_events": true,
                                      "push_events_branch_filter": "release/*",
                                      "branch_filter_strategy": "wildcard",
                                      "issues_events": false,
                                      "confidential_issues_events": false,
                                      "merge_requests_events": true,
                                      "tag_push_events": false,
                                      "note_events": false,
                                      "confidential_note_events": false,
                                      "job_events": false,
                                      "pipeline_events": true,
                                      "wiki_page_events": false,
                                      "deployment_events": false,
                                      "feature_flag_events": false,
                                      "releases_events": true,
                                      "milestone_events": false,
                                      "subgroup_events": true,
                                      "project_events": true,
                                      "member_events": true,
                                      "emoji_events": false,
                                      "resource_access_token_events": true,
                                      "vulnerability_events": false,
                                      "repository_update_events": false,
                                      "duo_flow_callback_enabled": false,
                                      "enable_ssl_verification": true,
                                      "alert_status": "executable",
                                      "disabled_until": null,
                                      "token_present": true,
                                      "signing_token_present": false,
                                      "custom_webhook_template": "{\"text\":\"{{object_kind}}\"}",
                                      "url_variables": [ { "key": "release_token" } ],
                                      "custom_headers": [ { "key": "X-Team" } ]
                                    }
                                    """;

    [Fact]
    public async Task ListAsync_BuildsGroupHooksRoute_AndDeserializesTheGroupOnlyTriggers()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{HookJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        List<GitLabGroupHook> hooks = new();
        await foreach (GitLabGroupHook item in repository.ListAsync(9, TestContext.Current.CancellationToken))
        {
            hooks.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGroupHook hook = Assert.Single(hooks);
        Assert.Equal(3, hook.Id);
        Assert.Equal(9, hook.GroupId);
        Assert.Equal(new Uri("https://example.com/group-hook"), hook.Url);
        Assert.Equal("Release notifier", hook.Name);
        Assert.True(hook.SubgroupEvents);
        Assert.True(hook.ProjectEvents);
        Assert.True(hook.MemberEvents);
        Assert.True(hook.ResourceAccessTokenEvents);
        Assert.Equal("release/*", hook.PushEventsBranchFilter);
        Assert.Equal("wildcard", hook.BranchFilterStrategy);
        Assert.Equal("executable", hook.AlertStatus);
        Assert.Null(hook.DisabledUntil);
        Assert.Equal(DateTimeOffset.Parse("2024-05-01T10:00:00.000Z", CultureInfo.InvariantCulture), hook.CreatedAt);
    }

    [Fact]
    public async Task GetAsync_KeepsSecretsOutOfTheResponseModel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(HookJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        GitLabGroupHook hook =
            await repository.GetAsync("gitlab-org/subgroup", 3, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/hooks/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // GitLab reports only that the secrets exist, and returns variable and header keys without values.
        Assert.True(hook.TokenPresent);
        Assert.False(hook.SigningTokenPresent);

        GitLabHookUrlVariable variable = Assert.Single(hook.UrlVariables ?? []);
        Assert.Equal("release_token", variable.Key);
        Assert.Null(variable.Value);

        GitLabHookCustomHeader header = Assert.Single(hook.CustomHeaders ?? []);
        Assert.Equal("X-Team", header.Key);
        Assert.Null(header.Value);
    }

    [Fact]
    public async Task CreateAsync_SerializesTriggersVariablesAndTheBranchFilterStrategy()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(HookJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        CreateGroupHookRequest request = new()
        {
            Url = new Uri("https://example.com/group-hook"),
            Name = "Release notifier",
            SubgroupEvents = true,
            MemberEvents = true,
            BranchFilterStrategy = GitLabHookBranchFilterStrategy.AllBranches,
            Token = "s3cret",
            UrlVariables = [new GitLabHookUrlVariable { Key = "release_token", Value = "abc" }],
            CustomHeaders = [new GitLabHookCustomHeader { Key = "X-Team", Value = "platform" }]
        };

        GitLabGroupHook hook = await repository.CreateAsync(9, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.Contains("\"url\":\"https://example.com/group-hook\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"subgroup_events\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"member_events\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"branch_filter_strategy\":\"all_branches\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"url_variables\":[{\"key\":\"release_token\",\"value\":\"abc\"}]", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"custom_headers\":[{\"key\":\"X-Team\",\"value\":\"platform\"}]", sentBody,
            StringComparison.Ordinal);

        // Unset trigger flags are omitted rather than sent as false, so GitLab applies its own defaults.
        Assert.DoesNotContain("\"issues_events\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(3, hook.Id);
    }

    [Fact]
    public async Task UpdateAsync_OmitsTheUrlWhenOnlyTriggersChange()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(HookJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        UpdateGroupHookRequest request = new() { PipelineEvents = true, EnableSslVerification = false };

        GitLabGroupHook hook = await repository.UpdateAsync(9, 3, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"pipeline_events":true,"enable_ssl_verification":false}""", sentBody);
        Assert.True(hook.PipelineEvents);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteAndIgnoresTheEchoedHookBody()
    {
        // The spec advertises 200 with the deleted hook here, unlike the 204 most delete endpoints answer.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(HookJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        await repository.DeleteAsync(9, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnFreeTierResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupHooksClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(9, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}