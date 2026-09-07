using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ExperimentsRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_RequestsTheExperimentsRoute_AndDeserializesNestedShapes()
    {
        const string Json = """
                            [
                              {
                                "key": "code_quality_walkthrough",
                                "context": ["user"],
                                "definition": {
                                  "name": "code_quality_walkthrough",
                                  "type": "experiment",
                                  "group": "group::adoption",
                                  "default_enabled": false
                                },
                                "current_status": {
                                  "state": "on",
                                  "gates": { "key": "percentage_of_actors", "value": 50 }
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        List<GitLabExperiment> experiments = new();
        await foreach (GitLabExperiment experiment in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            experiments.Add(experiment);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/experiments", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabExperiment experiment2 = Assert.Single(experiments);
        Assert.Equal("code_quality_walkthrough", experiment2.Key);
        Assert.Equal(["user"], experiment2.Context);
        Assert.Equal("experiment", experiment2.Definition?.Type);
        Assert.False(experiment2.Definition?.DefaultEnabled);
        Assert.Equal("on", experiment2.CurrentStatus?.State);
        Assert.Equal("percentage_of_actors", experiment2.CurrentStatus?.Gates?.Key);
    }

    [Fact]
    public async Task GetAssignmentAsync_RequestsTheAssignmentsRoute_WithoutAQueryString_WhenContextIsOmitted()
    {
        const string Json = """
                            {
                              "experiment": "lightweight_trial_registration_redesign",
                              "variant": "candidate",
                              "context_key": "abc123def456",
                              "cached": true
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        GitLabExperimentAssignment assignment = await repository.GetAssignmentAsync(
            "lightweight_trial_registration_redesign", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/experiments/lightweight_trial_registration_redesign/assignments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("candidate", assignment.Variant);
        Assert.Equal("abc123def456", assignment.ContextKey);
        Assert.True(assignment.Cached);
    }

    [Fact]
    public async Task GetAssignmentAsync_ProjectsTheContextDictionary_AsBracketedQueryParameters()
    {
        const string Json = """{ "experiment": "e", "variant": "control", "context_key": "k", "cached": false }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        await repository.GetAssignmentAsync("e", new Dictionary<string, string> { ["project"] = "42" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/experiments/e/assignments?context[project]=42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAssignmentAsync_EscapesASlashBearingExperimentName()
    {
        const string Json = """{ "experiment": "e", "variant": "control", "context_key": "k", "cached": false }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        await repository.GetAssignmentAsync("group/experiment",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/experiments/group%2Fexperiment/assignments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ForceAssignmentAsync_PostsTheVariantAndContext_AndReturnsTheAssignment()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "experiment": "e", "variant": "candidate", "context_key": "k", "cached": false }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        ForceExperimentAssignmentRequest request = new()
        {
            Variant = "candidate", Context = new Dictionary<string, string> { ["user"] = "7" }
        };

        GitLabExperimentAssignment assignment =
            await repository.ForceAssignmentAsync("e", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/experiments/e/assignments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"variant\":\"candidate\"", sentBody);
        Assert.Contains("\"context\":{\"user\":\"7\"}", sentBody);
        Assert.Equal("candidate", assignment.Variant);
    }

    [Fact]
    public async Task ClearAssignmentAsync_SendsDelete_ToTheAssignmentsRoute_WithTheContextQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        await repository.ClearAssignmentAsync("e", new Dictionary<string, string> { ["user"] = "7" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/experiments/e/assignments?context[user]=7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteCacheAsync_SendsDelete_ToTheCacheRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        await repository.DeleteCacheAsync("stale_experiment", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/experiments/stale_experiment/cache",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAssignmentAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ExperimentsRepository repository = new(connection);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAssignmentAsync("missing", cancellationToken: TestContext.Current.CancellationToken));
    }
}