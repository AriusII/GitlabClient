using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BroadcastMessagesRepositoryTests
{
    private const string BroadcastMessageJson = """
                                                {
                                                  "id": 1,
                                                  "message": "Update to 20.0 for a security fix",
                                                  "starts_at": "2016-01-04T15:39:55.570Z",
                                                  "ends_at": "2016-01-06T15:39:55.570Z",
                                                  "color": "#E75E40",
                                                  "font": "#FFFFFF",
                                                  "target_access_levels": [10, 30],
                                                  "target_path": "*/welcome",
                                                  "broadcast_type": "banner",
                                                  "theme": "indigo",
                                                  "dismissable": true,
                                                  "active": true
                                                }
                                                """;

    [Fact]
    public async Task ListAsync_BuildsBroadcastMessagesRoute_WithPaging_AndDeserializes()
    {
        string json = $"[{BroadcastMessageJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        List<GitLabBroadcastMessage> messages = new();
        await foreach (GitLabBroadcastMessage item in repository.ListAsync(
                           new BroadcastMessageListOptions { Page = 2, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            messages.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/broadcast_messages?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBroadcastMessage message = Assert.Single(messages);
        Assert.Equal(1, message.Id);
        Assert.Equal("Update to 20.0 for a security fix", message.Message);
        Assert.Equal("banner", message.BroadcastType);
        Assert.Equal("indigo", message.Theme);
        Assert.True(message.Dismissable);
        Assert.True(message.Active);
        Assert.Equal([10, 30], message.TargetAccessLevels);
        Assert.Equal("*/welcome", message.TargetPath);
    }

    [Fact]
    public async Task GetAsync_BuildsRouteWithId_AndDeserializes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BroadcastMessageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        GitLabBroadcastMessage message = await repository.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/broadcast_messages/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, message.Id);
    }

    [Fact]
    public async Task CreateAsync_PostsMessageWithEnumsAndAccessLevels_InDeclarationOrder()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BroadcastMessageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        CreateBroadcastMessageRequest request = new()
        {
            Message = "Update to 20.0 for a security fix",
            TargetAccessLevels = [10, 30],
            BroadcastType = GitLabBroadcastMessageType.Banner,
            Theme = GitLabBroadcastMessageTheme.Indigo
        };

        GitLabBroadcastMessage message =
            await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/broadcast_messages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // Enum members serialize through their wire value, not their C# name; the theme's hyphenated
        // values ("light-indigo") are the reason a plain naming policy could not cover this field.
        Assert.Equal(
            """
            {"message":"Update to 20.0 for a security fix","target_access_levels":[10,30],"broadcast_type":"banner","theme":"indigo"}
            """,
            sentBody);

        Assert.Equal("banner", message.BroadcastType);
    }

    [Fact]
    public async Task UpdateAsync_PutsToIdRoute_OmittingUnsetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BroadcastMessageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        UpdateBroadcastMessageRequest request = new()
        {
            TargetPath = "*/welcome", Dismissable = false, Theme = GitLabBroadcastMessageTheme.Dark
        };

        GitLabBroadcastMessage message =
            await repository.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/broadcast_messages/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"target_path":"*/welcome","dismissable":false,"theme":"dark"}""", sentBody);
        Assert.Equal(1, message.Id);
    }

    [Fact]
    public async Task DeleteAsync_SendsDelete_AndReturnsTheDeletedMessage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BroadcastMessageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        GitLabBroadcastMessage message = await repository.DeleteAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/broadcast_messages/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Update to 20.0 for a security fix", message.Message);
    }

    [Fact]
    public async Task GetAsync_OnMissingMessage_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BroadcastMessagesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}