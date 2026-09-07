using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class FeaturesRepositoryTests
{
    private const string FeaturesJson = """
                                        [
                                          {
                                            "name": "experimental_feature",
                                            "state": "off",
                                            "gates": [
                                              { "key": "boolean", "value": false }
                                            ]
                                          },
                                          {
                                            "name": "new_library",
                                            "state": "conditional",
                                            "gates": [
                                              { "key": "boolean", "value": false },
                                              { "key": "percentage_of_time", "value": 30 },
                                              { "key": "actors", "value": ["Project:1", "Group:2"] }
                                            ],
                                            "definition": {
                                              "name": "new_library",
                                              "introduced_by_url": "https://gitlab.com/gitlab-org/gitlab/-/merge_requests/1",
                                              "rollout_issue_url": "https://gitlab.com/gitlab-org/gitlab/-/issues/2",
                                              "milestone": "19.4",
                                              "type": "development",
                                              "group": "group::feature flags",
                                              "default_enabled": false,
                                              "log_state_changes": true
                                            }
                                          }
                                        ]
                                        """;

    [Fact]
    public async Task ListAsync_BuildsTheInstanceFeaturesRoute_AndReadsPolymorphicGateValues()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FeaturesJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        List<GitLabFeature> features = new();
        await foreach (GitLabFeature feature in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            features.Add(feature);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/features", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, features.Count);
        Assert.Equal("experimental_feature", features[0].Name);
        Assert.Equal("off", features[0].State);

        GitLabFeature conditional = features[1];
        Assert.Equal(3, conditional.Gates!.Count);

        // The spec types a gate value as an integer, but GitLab really answers with a boolean, a number
        // or an array depending on the gate - all three must read back without throwing.
        Assert.Equal(JsonValueKind.False, conditional.Gates[0].Value?.ValueKind);
        Assert.Equal(30, conditional.Gates[1].Value?.GetInt32());
        Assert.Equal(JsonValueKind.Array, conditional.Gates[2].Value?.ValueKind);

        Assert.Equal("19.4", conditional.Definition?.Milestone);
        Assert.Equal("group::feature flags", conditional.Definition?.Group);
        Assert.Equal(new Uri("https://gitlab.com/gitlab-org/gitlab/-/issues/2"),
            conditional.Definition?.RolloutIssueUrl);
    }

    [Fact]
    public async Task ListDefinitionsAsync_BuildsTheDefinitionsRoute()
    {
        const string Json = """
                            [
                              {
                                "name": "geo_registry_replication",
                                "feature_issue_url": "https://gitlab.com/gitlab-org/gitlab/-/issues/1",
                                "introduced_by_url": "https://gitlab.com/gitlab-org/gitlab/-/merge_requests/1",
                                "milestone": "19.0",
                                "type": "ops",
                                "group": "group::geo",
                                "default_enabled": true,
                                "intended_to_rollout_by": "19.6"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        List<GitLabFeatureDefinition> definitions = new();
        await foreach (GitLabFeatureDefinition definition in
                       repository.ListDefinitionsAsync(TestContext.Current.CancellationToken))
        {
            definitions.Add(definition);
        }

        Assert.Equal("https://gitlab.example/api/v4/features/definitions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabFeatureDefinition single = Assert.Single(definitions);
        Assert.Equal("geo_registry_replication", single.Name);
        Assert.Equal("ops", single.Type);
        Assert.True(single.DefaultEnabled);
        Assert.Equal("19.6", single.IntendedToRolloutBy);
    }

    [Fact]
    public async Task SetAsync_EscapesTheFeatureName_AndSendsABooleanValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"name":"new/library","state":"on","gates":[]}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        GitLabFeature feature = await repository.SetAsync("new/library", SetFeatureRequest.Enable(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/features/new%2Flibrary",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "value" must go out as a JSON boolean, not the string "true".
        Assert.Equal("""{"value":true}""", sentBody);
        Assert.Equal("on", feature.State);
    }

    [Fact]
    public async Task SetAsync_ForPercentage_SendsANumericValueAndTheGateKey()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"name":"new_library","state":"conditional","gates":[]}""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        SetFeatureRequest request = SetFeatureRequest.ForPercentage(30) with { Key = "percentage_of_actors" };

        await repository.SetAsync("new_library", request, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/features/new_library",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"value":30,"key":"percentage_of_actors"}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_EscapesTheFeatureName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        await repository.DeleteAsync("new/library", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/features/new%2Flibrary",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetUnleashFeaturesAsync_BuildsTheUnleashRoute_AndHandsBackTheRawDocument()
    {
        const string Json = """
                            {
                              "version": 1,
                              "features": [
                                {
                                  "name": "merge_train",
                                  "description": null,
                                  "enabled": true,
                                  "strategies": [
                                    { "name": "default", "parameters": {} }
                                  ]
                                }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        UnleashClientOptions options = new() { InstanceId = "abc 123", AppName = "production" };

        JsonElement document =
            await repository.GetUnleashFeaturesAsync(1, options, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/feature_flags/unleash/1?instance_id=abc%20123&app_name=production",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, document.GetProperty("version").GetInt32());
        JsonElement feature = Assert.Single(document.GetProperty("features").EnumerateArray().ToList());
        Assert.Equal("merge_train", feature.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetUnleashClientFeaturesAsync_BuildsTheClientFeaturesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"version":1,"features":[]}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        JsonElement document =
            await repository.GetUnleashClientFeaturesAsync(42,
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/feature_flags/unleash/42/client/features",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Empty(document.GetProperty("features").EnumerateArray().ToList());
    }

    [Fact]
    public async Task RegisterUnleashClientAsync_PostsTheClientIdentity()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        UnleashClientRegistrationRequest request = new() { InstanceId = "instance-1", AppName = "production" };

        await repository.RegisterUnleashClientAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/feature_flags/unleash/1/client/register",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"instance_id":"instance-1","app_name":"production"}""", sentBody);
    }

    [Fact]
    public async Task ReportUnleashMetricsAsync_PostsToTheMetricsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        await repository.ReportUnleashMetricsAsync("gitlab-org/gitlab",
            new UnleashClientRegistrationRequest { AppName = "production" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/feature_flags/unleash/gitlab-org%2Fgitlab/client/metrics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetAsync_WithoutAdministratorAccess_ThrowsGitLabForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{ "message": "403 Forbidden" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeaturesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.SetAsync("new_library", SetFeatureRequest.Enable(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}