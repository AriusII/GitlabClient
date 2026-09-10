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

public sealed class SystemHooksEndpointTests
{
    private const string HookJson = """
                                    {
                                      "id": 3,
                                      "url": "https://example.com/system-hook",
                                      "name": "Audit sink",
                                      "description": "Mirrors instance activity",
                                      "created_at": "2024-05-01T10:00:00.000Z",
                                      "push_events": false,
                                      "tag_push_events": true,
                                      "merge_requests_events": true,
                                      "repository_update_events": true,
                                      "enable_ssl_verification": true,
                                      "organization_id": 1,
                                      "alert_status": "temporarily_disabled",
                                      "disabled_until": "2024-05-02T10:00:00.000Z",
                                      "push_events_branch_filter": "main",
                                      "branch_filter_strategy": "regex",
                                      "custom_webhook_template": null,
                                      "token_present": true,
                                      "signing_token_present": true,
                                      "url_variables": [ { "key": "audit_token" } ],
                                      "custom_headers": [ { "key": "X-Audit" } ]
                                    }
                                    """;

    [Fact]
    public async Task ListAsync_BuildsTheInstanceHooksRoute_AndDeserializes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{HookJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        List<GitLabSystemHook> hooks = new();
        await foreach (GitLabSystemHook item in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            hooks.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSystemHook hook = Assert.Single(hooks);
        Assert.Equal(3, hook.Id);
        Assert.Equal(new Uri("https://example.com/system-hook"), hook.Url);
        Assert.Equal("Audit sink", hook.Name);
        Assert.Equal(1, hook.OrganizationId);
        Assert.True(hook.RepositoryUpdateEvents);
        Assert.Equal("regex", hook.BranchFilterStrategy);
        Assert.Equal("temporarily_disabled", hook.AlertStatus);
        Assert.Equal(DateTimeOffset.Parse("2024-05-02T10:00:00.000Z", CultureInfo.InvariantCulture),
            hook.DisabledUntil);
        Assert.Equal(DateTimeOffset.Parse("2024-05-01T10:00:00.000Z", CultureInfo.InvariantCulture), hook.CreatedAt);
        Assert.True(hook.TokenPresent);
        Assert.True(hook.SigningTokenPresent);
        Assert.Null(hook.CustomWebhookTemplate);
        Assert.Equal("audit_token", Assert.Single(hook.UrlVariables ?? []).Key);
        Assert.Equal("X-Audit", Assert.Single(hook.CustomHeaders ?? []).Key);
    }

    [Fact]
    public async Task GetAsync_BuildsTheHookRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(HookJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        GitLabSystemHook hook = await repository.GetAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, hook.Id);
    }

    [Fact]
    public async Task CreateAsync_PostsTheHookAndKeepsTheTokenOutOfTheResponseModel()
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
        SystemHooksClient repository = new(connection);

        CreateSystemHookRequest request = new()
        {
            Url = new Uri("https://example.com/system-hook"),
            Name = "Audit sink",
            RepositoryUpdateEvents = true,
            BranchFilterStrategy = GitLabHookBranchFilterStrategy.Regex,
            Token = "s3cret",
            SigningToken = "whsec_abc",
            UrlVariables = [new GitLabHookUrlVariable { Key = "audit_token", Value = "abc" }]
        };

        GitLabSystemHook hook = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"repository_update_events\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"branch_filter_strategy\":\"regex\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"token\":\"s3cret\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"signing_token\":\"whsec_abc\"", sentBody, StringComparison.Ordinal);

        Assert.True(hook.TokenPresent);
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlyWhatChanged()
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
        SystemHooksClient repository = new(connection);

        UpdateSystemHookRequest request = new() { MergeRequestsEvents = true };

        await repository.UpdateAsync(3, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"merge_requests_events":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheHookRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.DeleteAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task TestAsync_PostsToTheHookRouteItself_WithNoBodyAndNoTrigger()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.TestAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task DeleteUrlVariableAsync_SendsDeleteToTheUrlVariableRoute_EscapingTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.DeleteUrlVariableAsync(3, "audit/token", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/url_variables/audit%2Ftoken",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteCustomHeaderAsync_SendsDeleteToTheCustomHeaderRoute_EscapingTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.DeleteCustomHeaderAsync(3, "X-Audit", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/custom_headers/X-Audit",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateUrlVariableAsync_PutsToTheUrlVariableRoute_EscapingTheKey_WithNoResponseBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.UpdateUrlVariableAsync(3, "audit/token",
            new UpdateSystemHookUrlVariableRequest { Value = "s3cret" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/url_variables/audit%2Ftoken",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"value":"s3cret"}""", sentBody);
    }

    [Fact]
    public async Task UpdateCustomHeaderAsync_PutsToTheCustomHeaderRoute_EscapingTheKey_WithNoResponseBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        await repository.UpdateCustomHeaderAsync(3, "X-Audit",
            new UpdateSystemHookCustomHeaderRequest { Value = "hdr-value" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/hooks/3/custom_headers/X-Audit",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"value":"hdr-value"}""", sentBody);
    }

    [Fact]
    public void UpdateSystemHookUrlVariableRequest_ToString_RedactsTheValue()
    {
        UpdateSystemHookUrlVariableRequest request = new() { Value = "s3cret" };

        string rendered = request.ToString();

        Assert.DoesNotContain("s3cret", rendered, StringComparison.Ordinal);
        Assert.Contains("redacted", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdateSystemHookCustomHeaderRequest_ToString_RedactsTheValue()
    {
        UpdateSystemHookCustomHeaderRequest request = new() { Value = "hdr-value" };

        string rendered = request.ToString();

        Assert.DoesNotContain("hdr-value", rendered, StringComparison.Ordinal);
        Assert.Contains("redacted", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateUrlVariableAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.UpdateUrlVariableAsync(3, "missing",
                new UpdateSystemHookUrlVariableRequest { Value = "s3cret" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task ListAsync_WithoutAdminRights_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SystemHooksClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabSystemHook _ in repository
                               .ListAsync(TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The exception is thrown while fetching the first page, before any item is yielded.
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }
}