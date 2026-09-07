using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ResourceGroupsRepositoryTests
{
    private const string ResourceGroupJson = """
                                             {
                                               "id": 3,
                                               "key": "production",
                                               "process_mode": "unordered",
                                               "created_at": "2021-09-01T08:04:59.650Z",
                                               "updated_at": "2021-09-01T08:04:59.650Z"
                                             }
                                             """;

    private const string JobJson = """
                                   {
                                     "id": 244,
                                     "status": "running",
                                     "stage": "deploy",
                                     "name": "deploy_to_production",
                                     "ref": "main",
                                     "tag": false,
                                     "coverage": null,
                                     "allow_failure": false,
                                     "created_at": "2021-09-01T08:04:59.650Z",
                                     "started_at": "2021-09-01T08:05:12.000Z",
                                     "finished_at": null,
                                     "erased_at": null,
                                     "duration": 12.5,
                                     "queued_duration": 0.7,
                                     "user": {
                                       "id": 1,
                                       "username": "root",
                                       "name": "Administrator",
                                       "state": "active",
                                       "web_url": "https://gitlab.example/root"
                                     },
                                     "failure_reason": null,
                                     "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/244"
                                   }
                                   """;

    [Fact]
    public async Task ListAsync_BuildsResourceGroupsRoute_AndDeserializesTheProcessMode()
    {
        string json = $"[{ResourceGroupJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        List<GitLabResourceGroup> groups = new();
        await foreach (GitLabResourceGroup item in repository.ListAsync(1, TestContext.Current.CancellationToken))
        {
            groups.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/resource_groups",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceGroup group = Assert.Single(groups);
        Assert.Equal(3, group.Id);
        Assert.Equal("production", group.Key);
        Assert.Equal("unordered", group.ProcessMode);
        Assert.Equal(new DateTimeOffset(2021, 9, 1, 8, 4, 59, 650, TimeSpan.Zero), group.CreatedAt);
    }

    /// <summary>Resource group keys are free text from .gitlab-ci.yml and legally contain '/'.</summary>
    [Fact]
    public async Task GetAsync_EscapesSlashInTheKey_AndEncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ResourceGroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        GitLabResourceGroup group = await repository.GetAsync("gitlab-org/gitlab", "review/production",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/resource_groups/review%2Fproduction",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("production", group.Key);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheProcessMode()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResourceGroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        UpdateResourceGroupRequest request = new() { ProcessMode = "oldest_first" };

        await repository.UpdateAsync(1, "production", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/resource_groups/production",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"process_mode":"oldest_first"}""", sentBody);
    }

    [Fact]
    public async Task GetCurrentJobAsync_BuildsTheCurrentJobRoute_AndDeserializesTheJob()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JobJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        GitLabJob job = await repository.GetCurrentJobAsync(1, "production", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/resource_groups/production/current_job",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(244, job.Id);
        Assert.Equal("running", job.Status);
        Assert.Equal("deploy_to_production", job.Name);
        Assert.Equal(12.5, job.Duration);
        Assert.Equal("root", job.User?.Username);
    }

    [Fact]
    public async Task ListUpcomingJobsAsync_BuildsTheUpcomingJobsRoute()
    {
        string json = $"[{JobJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        List<GitLabJob> jobs = new();
        await foreach (GitLabJob item in repository.ListUpcomingJobsAsync(1, "review/production",
                           TestContext.Current.CancellationToken))
        {
            jobs.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/resource_groups/review%2Fproduction/upcoming_jobs",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(244, Assert.Single(jobs).Id);
    }

    [Fact]
    public async Task UpdateAsync_OnUnknownProcessMode_ThrowsGitLabValidationException()
    {
        const string Json = """{ "error": "process_mode does not have a valid value" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceGroupsRepository repository = new(connection);

        UpdateResourceGroupRequest request = new() { ProcessMode = "random" };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.UpdateAsync(1, "production", request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}