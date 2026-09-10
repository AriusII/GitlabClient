using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class PushRulesEndpointTests
{
    private const string ProjectPushRuleJson = """
                                               {
                                                 "id": 1,
                                                 "project_id": 3,
                                                 "created_at": "2020-08-31T15:53:00.073Z",
                                                 "commit_message_regex": "Fixes \\d+\\..*",
                                                 "commit_message_negative_regex": "ssh\\:\\/\\/",
                                                 "branch_name_regex": "",
                                                 "deny_delete_tag": false,
                                                 "member_check": false,
                                                 "prevent_secrets": false,
                                                 "author_email_regex": "@example\\.com$",
                                                 "file_name_regex": "(jar|exe)$",
                                                 "max_file_size": 5,
                                                 "commit_committer_check": false,
                                                 "commit_committer_name_check": false,
                                                 "reject_unsigned_commits": false,
                                                 "reject_non_dco_commits": false
                                               }
                                               """;

    private const string GroupPushRuleJson = """
                                             {
                                               "id": 9,
                                               "created_at": "2021-04-02T08:00:00.000Z",
                                               "commit_message_regex": "Fixes \\d+\\..*",
                                               "commit_message_negative_regex": null,
                                               "branch_name_regex": null,
                                               "author_email_regex": "@example\\.com$",
                                               "file_name_regex": null,
                                               "deny_delete_tag": true,
                                               "member_check": false,
                                               "prevent_secrets": true,
                                               "max_file_size": 10,
                                               "commit_committer_check": false,
                                               "commit_committer_name_check": false,
                                               "reject_unsigned_commits": false,
                                               "reject_non_dco_commits": false
                                             }
                                             """;

    [Fact]
    public async Task GetForProjectAsync_BuildsPushRuleRoute_AndDeserializesTheRule()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectPushRuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        GitLabProjectPushRule rule =
            await repository.GetForProjectAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, rule.Id);
        Assert.Equal(3, rule.ProjectId);
        Assert.Equal("Fixes \\d+\\..*", rule.CommitMessageRegex);
        Assert.Equal("@example\\.com$", rule.AuthorEmailRegex);
        Assert.Equal(5, rule.MaxFileSize);
        Assert.False(rule.DenyDeleteTag);
    }

    [Fact]
    public async Task GetForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectPushRuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        await repository.GetForProjectAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsOnlyTheSetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectPushRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        CreatePushRuleRequest request = new() { DenyDeleteTag = true, CommitMessageRegex = "must include JIRA-" };

        GitLabProjectPushRule rule =
            await repository.CreateForProjectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"deny_delete_tag":true,"commit_message_regex":"must include JIRA-"}""", sentBody);
        Assert.Equal(1, rule.Id);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsToThePushRuleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ProjectPushRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        UpdatePushRuleRequest request = new() { MaxFileSize = 5 };

        GitLabProjectPushRule rule = await repository.UpdateForProjectAsync("gitlab-org/gitlab", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"max_file_size":5}""", sentBody);
        Assert.Equal(5, rule.MaxFileSize);
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToThePushRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        await repository.DeleteForProjectAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupPushRuleRoute_AndDeserializesTheRule()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupPushRuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        GitLabGroupPushRule rule =
            await repository.GetForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(9, rule.Id);
        Assert.True(rule.DenyDeleteTag);
        Assert.True(rule.PreventSecrets);
        Assert.Null(rule.BranchNameRegex);
        Assert.Equal(10, rule.MaxFileSize);
    }

    [Fact]
    public async Task GetForGroupAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupPushRuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        await repository.GetForGroupAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheGroupPushRuleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupPushRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        CreatePushRuleRequest request = new() { PreventSecrets = true, MaxFileSize = 10 };

        GitLabGroupPushRule rule =
            await repository.CreateForGroupAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"prevent_secrets":true,"max_file_size":10}""", sentBody);
        Assert.Equal(9, rule.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheGroupPushRuleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupPushRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        UpdatePushRuleRequest request = new() { DenyDeleteTag = true };

        await repository.UpdateForGroupAsync("parent-group/subgroup", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"deny_delete_tag":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToTheGroupPushRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        await repository.DeleteForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/push_rule",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_OnMissingPushRule_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "base": ["Push Rule already exists"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PushRulesClient repository = new(connection);

        CreatePushRuleRequest request = new() { DenyDeleteTag = true };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForProjectAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("Push Rule already exists", exception.Message, StringComparison.Ordinal);
    }
}