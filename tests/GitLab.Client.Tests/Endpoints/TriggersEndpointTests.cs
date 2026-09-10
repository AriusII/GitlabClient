using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class TriggersEndpointTests
{
    private const string TriggerJson = """
                                       {
                                         "id": 10,
                                         "description": "my trigger",
                                         "created_at": "2016-01-07T09:53:58.235Z",
                                         "last_used": "2016-01-08T10:00:00.000Z",
                                         "token": "6d056f63e50fe6f8c5f8f4aa10edb7",
                                         "updated_at": "2016-01-07T09:53:58.235Z",
                                         "expires_at": "2026-12-31T23:59:59.000Z",
                                         "owner": {
                                           "id": 1,
                                           "username": "root",
                                           "name": "Administrator",
                                           "state": "active",
                                           "web_url": "https://gitlab.example/root"
                                         }
                                       }
                                       """;

    private const string PipelineJson = """
                                        {
                                          "id": 83,
                                          "iid": 12,
                                          "project_id": 1,
                                          "sha": "0ff3ae198f8601a285adcf5c0fff204ee6fba5fd",
                                          "ref": "feature/x",
                                          "status": "pending",
                                          "source": "trigger",
                                          "created_at": "2016-08-11T11:28:34.085Z",
                                          "updated_at": "2016-08-11T11:32:35.169Z",
                                          "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/83"
                                        }
                                        """;

    [Fact]
    public async Task ListAsync_BuildsTriggersRoute_AndDeserializesEachTrigger()
    {
        const string Json = $"[{TriggerJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        List<GitLabTrigger> triggers = new();
        await foreach (GitLabTrigger item in repository.ListAsync(1, TestContext.Current.CancellationToken))
        {
            triggers.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/triggers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabTrigger trigger = Assert.Single(triggers);
        Assert.Equal(10, trigger.Id);
        Assert.Equal("my trigger", trigger.Description);
        Assert.Equal("6d056f63e50fe6f8c5f8f4aa10edb7", trigger.Token);
        Assert.Equal(new DateTimeOffset(2016, 1, 7, 9, 53, 58, 235, TimeSpan.Zero), trigger.CreatedAt);
        Assert.Equal(new DateTimeOffset(2016, 1, 8, 10, 0, 0, TimeSpan.Zero), trigger.LastUsed);
        Assert.Equal(new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero), trigger.ExpiresAt);
        Assert.NotNull(trigger.Owner);
        Assert.Equal("root", trigger.Owner!.Username);
    }

    [Fact]
    public async Task GetAsync_BuildsTriggerIdRoute_AndDeserializesTrigger()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TriggerJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        GitLabTrigger trigger = await repository.GetAsync(1, 10, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/triggers/10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(10, trigger.Id);
        Assert.Equal("my trigger", trigger.Description);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TriggerJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", 10, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/triggers/10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsSerializedBody_AndDeserializesCreatedTrigger()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TriggerJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        CreateTriggerRequest request = new()
        {
            Description = "my trigger", ExpiresAt = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero)
        };

        GitLabTrigger trigger = await repository.CreateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/triggers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"description\":\"my trigger\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-12-31T23:59:59+00:00\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(10, trigger.Id);
        Assert.Equal("6d056f63e50fe6f8c5f8f4aa10edb7", trigger.Token);
    }

    [Fact]
    public async Task CreateAsync_WithoutExpiry_OmitsExpiresAt()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TriggerJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        await repository.CreateAsync(1, new CreateTriggerRequest { Description = "no expiry" },
            TestContext.Current.CancellationToken);

        Assert.Equal("{\"description\":\"no expiry\"}", sentBody);
    }

    [Fact]
    public async Task UpdateAsync_PutsNewDescription_ToTheTriggerIdRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TriggerJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        GitLabTrigger trigger = await repository.UpdateAsync(1, 10,
            new UpdateTriggerRequest { Description = "renamed trigger" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/triggers/10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"description\":\"renamed trigger\"}", sentBody);
        Assert.Equal(10, trigger.Id);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheTriggerIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        await repository.DeleteAsync(1, 10, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/triggers/10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>The singular <c>trigger</c> segment here is not a typo - token management uses the plural.</summary>
    [Fact]
    public async Task TriggerPipelineAsync_PostsTokenVariablesAndTypedInputs_ToTheSingularTriggerRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(PipelineJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        TriggerPipelineRequest request = new()
        {
            Token = "6d056f63e50fe6f8c5f8f4aa10edb7",
            Variables = new Dictionary<string, string>(StringComparer.Ordinal) { ["DEPLOY_ENV"] = "staging" },
            Inputs = new Dictionary<string, GitLabPipelineInputValue>(StringComparer.Ordinal)
            {
                ["DEPLOY_TIER"] = GitLabPipelineInputValue.Text("staging"),
                ["DRY_RUN"] = GitLabPipelineInputValue.Flag(true),
                ["REGIONS"] = GitLabPipelineInputValue.Sequence(
                    [GitLabPipelineInputValue.Text("eu"), GitLabPipelineInputValue.Text("us")])
            }
        };

        GitLabPipeline pipeline =
            await repository.TriggerPipelineAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/trigger/pipeline",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"token\":\"6d056f63e50fe6f8c5f8f4aa10edb7\"", sentBody, StringComparison.Ordinal);

        // A JSON object map, not the variables[KEY]=value form encoding the curl examples show.
        Assert.Contains("\"variables\":{\"DEPLOY_ENV\":\"staging\"}", sentBody, StringComparison.Ordinal);

        using JsonDocument sent = JsonDocument.Parse(sentBody!);
        JsonElement inputs = sent.RootElement.GetProperty("inputs");
        Assert.Equal("staging", inputs.GetProperty("DEPLOY_TIER").GetString());
        Assert.True(inputs.GetProperty("DRY_RUN").GetBoolean());
        Assert.Equal("eu", inputs.GetProperty("REGIONS")[0].GetString());
        Assert.Equal("us", inputs.GetProperty("REGIONS")[1].GetString());

        Assert.Equal(83, pipeline.Id);
        Assert.Equal("pending", pipeline.Status);
        Assert.Equal("feature/x", pipeline.Ref);
        Assert.Equal("trigger", pipeline.Source);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/pipelines/83"), pipeline.WebUrl);
    }

    [Fact]
    public async Task TriggerPipelineAsync_WithoutVariables_OmitsThem()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(PipelineJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        await repository.TriggerPipelineAsync(1, new TriggerPipelineRequest { Token = "abc" },
            TestContext.Current.CancellationToken);

        Assert.Equal("{\"token\":\"abc\"}", sentBody);
    }

    [Fact]
    public async Task TriggerPipelineForRefAsync_EscapesTheRef_AndEncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(PipelineJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        GitLabPipeline pipeline = await repository.TriggerPipelineForRefAsync("gitlab-org/gitlab", "feature/x",
            new TriggerPipelineRequest { Token = "abc" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/ref/feature%2Fx/trigger/pipeline",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("feature/x", pipeline.Ref);
        Assert.Equal(1, pipeline.ProjectId);
    }

    [Fact]
    public async Task TriggerPipelineAsync_OnMissingToken_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "400 Bad Request - token is missing" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.TriggerPipelineAsync(1, new TriggerPipelineRequest { Token = "expired" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Bad Request - token is missing", exception.Message);
    }

    [Fact]
    public async Task GetAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Trigger Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TriggersClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Trigger Not Found", exception.Message);
    }
}