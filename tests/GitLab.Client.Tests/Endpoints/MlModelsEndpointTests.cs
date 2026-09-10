using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class MlModelsEndpointTests
{
    private const string RegisteredModelJson = """
                                               {
                                                 "name": "sentiment/classifier",
                                                 "creation_timestamp": 1714560000000,
                                                 "last_updated_timestamp": 1714646400000,
                                                 "description": "Review sentiment classifier",
                                                 "user_id": "42",
                                                 "tags": [
                                                   { "key": "team", "value": "ml-platform" },
                                                   { "key": "archived" }
                                                 ],
                                                 "latest_versions": [
                                                   {
                                                     "name": "sentiment/classifier",
                                                     "version": "2.1.0",
                                                     "creation_timestamp": 1714646400000,
                                                     "last_updated_timestamp": 1714646400000,
                                                     "user_id": "42",
                                                     "current_stage": "development",
                                                     "description": "Retrained on Q2 data",
                                                     "source": "gitlab://packages/ml_model/9",
                                                     "run_id": "c1f2a3",
                                                     "status": "READY",
                                                     "status_message": null,
                                                     "tags": [{ "key": "framework", "value": "pytorch" }],
                                                     "run_link": "https://gitlab.example/g/p/-/ml/candidates/9",
                                                     "aliases": ["champion", "1.0.0"]
                                                   }
                                                 ]
                                               }
                                               """;

    private const string ModelVersionJson = """
                                            {
                                              "name": "sentiment/classifier",
                                              "version": "2.1.0",
                                              "creation_timestamp": 1714646400000,
                                              "current_stage": "development",
                                              "status": "READY",
                                              "aliases": ["champion"]
                                            }
                                            """;

    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string RegisteredModelsRoot =
        "https://gitlab.example/api/v4/projects/1/ml/mlflow/api/2.0/mlflow/registered-models";

    [Fact]
    public async Task GetAsync_BuildsTheMlflowRoute_AndDeserializesTheRegisteredModel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RegisteredModelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabMlModel model =
            await repository.GetAsync(1, "sentiment/classifier", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The MLflow protocol version "2.0" is a fixed path word from the route template, so it must
        // survive verbatim rather than being escaped.
        Assert.Equal($"{RegisteredModelsRoot}/get?name=sentiment%2Fclassifier",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("sentiment/classifier", model.Name);
        Assert.Equal(1714560000000, model.CreationTimestamp);
        Assert.Equal(1714646400000, model.LastUpdatedTimestamp);
        Assert.Equal("Review sentiment classifier", model.Description);
        Assert.Equal("42", model.UserId);

        Assert.NotNull(model.Tags);
        Assert.Equal(2, model.Tags!.Count);
        Assert.Equal("team", model.Tags[0].Key);
        Assert.Equal("ml-platform", model.Tags[0].Value);
        Assert.Equal("archived", model.Tags[1].Key);
        Assert.Null(model.Tags[1].Value);

        GitLabMlModelVersion version = Assert.Single(model.LatestVersions!);
        Assert.Equal("sentiment/classifier", version.Name);
        Assert.Equal("2.1.0", version.Version);
        Assert.Equal("development", version.CurrentStage);
        Assert.Equal("READY", version.Status);
        Assert.Null(version.StatusMessage);
        Assert.Equal("gitlab://packages/ml_model/9", version.Source);
        Assert.Equal("c1f2a3", version.RunId);
        Assert.Equal("https://gitlab.example/g/p/-/ml/candidates/9", version.RunLink);
        Assert.NotNull(version.Aliases);
        Assert.Equal(2, version.Aliases!.Count);
        Assert.Equal("champion", version.Aliases[0]);
        Assert.Equal("1.0.0", version.Aliases[1]);
        Assert.Equal("pytorch", Assert.Single(version.Tags!).Value);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RegisteredModelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", "model", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/ml/mlflow/api/2.0/mlflow/"
            + "registered-models/get?name=model",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheModelName_DescriptionAndTags()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RegisteredModelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        CreateMlModelRequest request = new()
        {
            Name = "sentiment/classifier",
            Description = "Review sentiment classifier",
            Tags = [new GitLabMlModelTag { Key = "team", Value = "ml-platform" }]
        };

        GitLabMlModel model = await repository.CreateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"{RegisteredModelsRoot}/create", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"name\":\"sentiment/classifier\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Review sentiment classifier\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"tags\":[{\"key\":\"team\",\"value\":\"ml-platform\"}]", sentBody, StringComparison.Ordinal);

        Assert.Equal("sentiment/classifier", model.Name);
    }

    [Fact]
    public async Task UpdateAsync_PatchesTheUpdateRoute_WithNameAndDescription()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RegisteredModelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        UpdateMlModelRequest request = new() { Name = "sentiment/classifier", Description = "Now trained on Q2 data" };

        await repository.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal($"{RegisteredModelsRoot}/update", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"name\":\"sentiment/classifier\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Now trained on Q2 data\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsync_SendsTheNameAsAQueryParameter_AndReturnsTheDeletedModel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RegisteredModelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabMlModel deleted =
            await repository.DeleteAsync(1, "sentiment/classifier", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal($"{RegisteredModelsRoot}/delete?name=sentiment%2Fclassifier",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("sentiment/classifier", deleted.Name);
    }

    [Fact]
    public async Task SearchAsync_ProjectsEveryFilter_AndReadsTheContinuationToken()
    {
        const string Json = $$"""
                              {
                                "registered_models": [{{RegisteredModelJson}}],
                                "next_page_token": "eyJvZmZzZXQiOjIwMH0"
                              }
                              """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        MlModelSearchOptions options = new()
        {
            Filter = "name='sentiment'",
            MaxResults = 50,
            OrderBy = "last_updated_timestamp DESC",
            PageToken = "eyJvZmZzZXQiOjB9"
        };

        GitLabMlModelSearchResult result =
            await repository.SearchAsync(1, options, TestContext.Current.CancellationToken);

        Assert.Equal(
            $"{RegisteredModelsRoot}/search?filter=name%3D%27sentiment%27&max_results=50"
            + "&order_by=last_updated_timestamp%20DESC&page_token=eyJvZmZzZXQiOjB9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("eyJvZmZzZXQiOjIwMH0", result.NextPageToken);
        Assert.Equal("sentiment/classifier", Assert.Single(result.RegisteredModels!).Name);
    }

    [Fact]
    public async Task SearchAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"registered_models":[]}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabMlModelSearchResult result =
            await repository.SearchAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"{RegisteredModelsRoot}/search", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Empty(result.RegisteredModels!);
        Assert.Null(result.NextPageToken);
    }

    [Fact]
    public async Task GetLatestVersionAsync_PostsTheNameInTheBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ModelVersionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabMlModelVersion version =
            await repository.GetLatestVersionAsync(1, "sentiment/classifier", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"{RegisteredModelsRoot}/get-latest-versions", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"sentiment/classifier"}""", sentBody);

        Assert.Equal("2.1.0", version.Version);
        Assert.Equal("champion", Assert.Single(version.Aliases!));
    }

    [Fact]
    public async Task GetVersionByAliasAsync_EscapesBothTheNameAndTheDottedAlias()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ModelVersionJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabMlModelVersion version = await repository.GetVersionByAliasAsync(
            1,
            "sentiment/classifier",
            "release 1.0.0",
            TestContext.Current.CancellationToken);

        // Model names and aliases are caller-supplied free text carrying slashes, dots and spaces. They
        // travel as query parameters on this resource, so GitLabRouteBuilder.Query owns the encoding:
        // the slash becomes %2F and the space %20, while the dots stay literal (they are legal in a
        // query and GitLab reads them back unchanged).
        Assert.Equal($"{RegisteredModelsRoot}/alias?name=sentiment%2Fclassifier&alias=release%201.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("sentiment/classifier", version.Name);
    }

    [Fact]
    public async Task GetAsync_MapsNotFoundToTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Model Not Found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        MlModelsClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}