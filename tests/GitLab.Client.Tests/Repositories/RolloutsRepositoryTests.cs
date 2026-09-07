using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class RolloutsRepositoryTests
{
    [Fact]
    public async Task IngestEventAsync_PostsToTheRolloutRoute_WithSerializedBody_AndDeserializesTheRollout()
    {
        const string Json = """
                            {
                              "id": 1,
                              "iid": 1,
                              "state": "in_progress",
                              "workflow_ref": "cd-rollout-1",
                              "started_at": "2026-07-22T11:37:00Z",
                              "finished_at": "2026-07-22T11:42:00Z"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RolloutsRepository repository = new(connection);

        IngestRolloutEventRequest request = new()
        {
            Topic = "cd.rollout",
            Type = "step_succeeded",
            Data = new RolloutEventPayload { Position = [0, 1], StageName = "staging", StepType = "deploy" },
            ChannelTokens = [new RolloutChannelToken { ChannelName = "approval", Token = "tok-123" }]
        };

        GitLabCdRollout rollout = await repository.IngestEventAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/rollouts/1", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Contains("\"topic\":\"cd.rollout\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"step_succeeded\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"position\":[0,1]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"stage_name\":\"staging\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"channel_name\":\"approval\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"token\":\"tok-123\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(1, rollout.Id);
        Assert.Equal("in_progress", rollout.State);
        Assert.Equal("cd-rollout-1", rollout.WorkflowRef);
        Assert.Equal(DateTimeOffset.Parse("2026-07-22T11:37:00Z", CultureInfo.InvariantCulture), rollout.StartedAt);
    }

    [Fact]
    public async Task IngestEventAsync_OnErrorResponse_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "Unknown rollout" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RolloutsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.IngestEventAsync(1, new IngestRolloutEventRequest(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Unknown rollout", exception.Message);
    }
}