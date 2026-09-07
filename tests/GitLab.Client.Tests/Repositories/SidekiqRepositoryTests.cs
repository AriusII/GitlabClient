using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SidekiqRepositoryTests
{
    [Fact]
    public async Task DeleteQueueJobsAsync_EscapesTheQueueName_AndAppliesMetadataFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SidekiqRepository repository = new(connection);

        SidekiqQueueJobDeleteOptions options = new()
        {
            OrganizationId = "1",
            GlUserId = "5",
            WorkerClass = "PostReceive",
            ArtifactUsedCdn = "true",
            MvccManifest = "abc",
            AiResource = "duo_chat"
        };

        await repository.DeleteQueueJobsAsync("high priority", options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/admin/sidekiq/queues/high%20priority"
            + "?organization_id=1&gl_user_id=5&artifact_used_cdn=true"
            + "&mvcc_manifest=abc&ai_resource=duo_chat&worker_class=PostReceive",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetQueueMetricsAsync_BuildsTheNonAdminRoute_AndReturnsTheRawPayload()
    {
        const string Json = """{ "queues": { "default": { "backlog": 3, "latency": 0.1 } } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SidekiqRepository repository = new(connection);

        JsonElement result = await repository.GetQueueMetricsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/sidekiq/queue_metrics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, result.GetProperty("queues").GetProperty("default").GetProperty("backlog").GetInt32());
    }

    [Fact]
    public async Task GetProcessMetricsAsync_BuildsTheNonAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SidekiqRepository repository = new(connection);

        await repository.GetProcessMetricsAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/sidekiq/process_metrics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetJobStatsAsync_BuildsTheNonAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SidekiqRepository repository = new(connection);

        await repository.GetJobStatsAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/sidekiq/job_stats", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetCompoundMetricsAsync_BuildsTheNonAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SidekiqRepository repository = new(connection);

        await repository.GetCompoundMetricsAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/sidekiq/compound_metrics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}