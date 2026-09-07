using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class NotificationSettingsRepositoryTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string CustomSettingsJson = """
                                              {
                                                "level": "custom",
                                                "notification_email": "user@example.com",
                                                "new_note": true,
                                                "new_issue": true,
                                                "reopen_issue": false,
                                                "close_merge_request": true,
                                                "failed_pipeline": true,
                                                "success_pipeline": false,
                                                "new_epic": true,
                                                "approver": true
                                              }
                                              """;

    [Fact]
    public async Task GetGlobalAsync_BuildsInstanceRoute_AndDeserializesCustomFlags()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CustomSettingsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabNotificationSettings settings =
            await repository.GetGlobalAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(GitLabNotificationLevel.Custom, settings.Level);
        Assert.Equal("user@example.com", settings.NotificationEmail);
        Assert.True(settings.NewNote);
        Assert.True(settings.NewIssue);
        Assert.False(settings.ReopenIssue);
        Assert.True(settings.CloseMergeRequest);
        Assert.True(settings.FailedPipeline);
        Assert.False(settings.SuccessPipeline);
        Assert.True(settings.NewEpic);
        Assert.True(settings.Approver);

        // Untouched flags stay null rather than defaulting to false.
        Assert.Null(settings.NewRelease);
        Assert.Null(settings.MergeMergeRequest);
    }

    [Fact]
    public async Task GetGlobalAsync_OnSimpleLevel_LeavesEventFlagsNull()
    {
        const string Json = """{ "level": "participating", "notification_email": "user@example.com" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabNotificationSettings settings =
            await repository.GetGlobalAsync(TestContext.Current.CancellationToken);

        Assert.Equal(GitLabNotificationLevel.Participating, settings.Level);
        Assert.Null(settings.NewNote);
        Assert.Null(settings.Approver);
    }

    [Fact]
    public async Task UpdateGlobalAsync_PutsToInstanceRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(CustomSettingsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        UpdateNotificationSettingsRequest request = new()
        {
            Level = GitLabNotificationLevel.Custom,
            NotificationEmail = "user@example.com",
            NewNote = true,
            Approver = true
        };

        GitLabNotificationSettings settings =
            await repository.UpdateGlobalAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"level":"custom","notification_email":"user@example.com","new_note":true,"approver":true}""",
            sentBody);
        Assert.Equal(GitLabNotificationLevel.Custom, settings.Level);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupRoute_EncodingNamespacedPath()
    {
        const string Json = """{ "level": "watch" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabNotificationSettings settings = await repository.GetForGroupAsync("parent-group/subgroup",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabNotificationLevel.Watch, settings.Level);
        Assert.Null(settings.NotificationEmail);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToGroupRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "level": "custom", "new_epic": true }""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        UpdateNotificationSettingsRequest request = new() { Level = GitLabNotificationLevel.Custom, NewEpic = true };

        GitLabNotificationSettings settings =
            await repository.UpdateForGroupAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"level":"custom","new_epic":true}""", sentBody);
        Assert.True(settings.NewEpic);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsProjectRoute()
    {
        const string Json = """{ "level": "disabled" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabNotificationSettings settings =
            await repository.GetForProjectAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabNotificationLevel.Disabled, settings.Level);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsToProjectRoute_EncodingNamespacedPath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "level": "custom", "approver": true }""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        UpdateNotificationSettingsRequest request = new() { Level = GitLabNotificationLevel.Custom, Approver = true };

        GitLabNotificationSettings settings = await repository.UpdateForProjectAsync("gitlab-org/gitlab", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/notification_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"level":"custom","approver":true}""", sentBody);
        Assert.True(settings.Approver);
    }

    [Fact]
    public async Task GetForProjectAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task UpdateGlobalAsync_WithoutAuthentication_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        NotificationSettingsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.UpdateGlobalAsync(
                new UpdateNotificationSettingsRequest { Level = GitLabNotificationLevel.Watch },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }
}