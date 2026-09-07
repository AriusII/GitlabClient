using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MlExperimentsRepositoryTests
{
    /// <summary>
    ///     The prefix every MLflow-compatible route hangs off. GitLab mounts MLflow's own API under the
    ///     project and versions it by MLflow's <c>2.0</c>, not by GitLab's <c>v4</c>.
    /// </summary>
    private const string MlflowPrefix = "https://gitlab.example/api/v4/projects/7/ml/mlflow/api/2.0";

    private const string ExperimentJson = """
                                          {
                                            "experiment_id": "12",
                                            "name": "churn-model",
                                            "lifecycle_stage": "active",
                                            "artifact_location": "mlflow-artifacts:12",
                                            "tags": [
                                              { "key": "framework", "value": "sklearn" },
                                              { "key": "owner", "value": null }
                                            ]
                                          }
                                          """;

    private const string RunJson = """
                                   {
                                     "info": {
                                       "run_id": "a1b2c3d4e5f6",
                                       "run_uuid": "a1b2c3d4e5f6",
                                       "experiment_id": "12",
                                       "start_time": 1717243800000,
                                       "end_time": 1717243900000,
                                       "run_name": "candidate-4",
                                       "status": "FINISHED",
                                       "artifact_uri": "mlflow-artifacts:12",
                                       "lifecycle_stage": "active",
                                       "user_id": ""
                                     },
                                     "data": {
                                       "metrics": [
                                         { "key": "rmse", "value": 0.25, "timestamp": 1717243850000, "step": 3 }
                                       ],
                                       "params": [
                                         { "key": "n_estimators", "value": "100" }
                                       ],
                                       "tags": [
                                         { "key": "stage", "value": "training" }
                                       ]
                                     }
                                   }
                                   """;

    private const string ModelVersionJson = """
                                            {
                                              "name": "churn-model",
                                              "version": "3",
                                              "creation_timestamp": 1717243800000,
                                              "last_updated_timestamp": 1717243900000,
                                              "user_id": "",
                                              "current_stage": "development",
                                              "description": "promoted from candidate-4",
                                              "source": "mlflow-artifacts:12",
                                              "run_id": "a1b2c3d4e5f6",
                                              "status": "READY",
                                              "status_message": "",
                                              "tags": [ { "key": "gpu", "value": "a100" } ],
                                              "run_link": "",
                                              "aliases": [ "champion" ]
                                            }
                                            """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    [Fact]
    public async Task DownloadArtifactAsync_BuildsTheMlflowArtifactsRoute_AndStreamsTheOpaqueBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("artifact-bytes", Encoding.UTF8, "application/octet-stream")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        using GitLabFileResponse artifact =
            await repository.DownloadArtifactAsync(7, "15/MLmodel", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // "mlflow-artifacts" is a sibling namespace of "mlflow" under the 2.0 segment, and the artifact
        // path is a query parameter rather than a route segment - so its slash is escaped by Query, not
        // turned into a path segment.
        Assert.Equal($"{MlflowPrefix}/mlflow-artifacts/artifacts?path=15%2FMLmodel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(artifact.Content);
        Assert.Equal("artifact-bytes", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DownloadArtifactAsync_OmitsThePathParameterEntirely_WhenItIsNotSupplied()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/octet-stream")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        using GitLabFileResponse _ =
            await repository.DownloadArtifactAsync(7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow-artifacts/artifacts", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadArtifactFileAsync_EscapesTheModelVersionAndTheFilePath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("flavor: python_function", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        using GitLabFileResponse _ = await repository.DownloadArtifactFileAsync(
            7, "1.0/rc", "model/data/MLmodel", TestContext.Current.CancellationToken);

        // Both segments are caller-supplied free text, so both go through .Escaped, never .Literal: an
        // unencoded slash would invent path segments and address a route that does not exist, which reads
        // back as a missing artifact rather than as a client bug.
        Assert.Equal($"{MlflowPrefix}/mlflow-artifacts/artifacts/1.0%2Frc/model%2Fdata%2FMLmodel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateExperimentAsync_PostsTheMlflowBody_AndReadsBackOnlyTheNewId()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json("""{ "experiment_id": "12" }""", HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowNewExperiment created = await repository.CreateExperimentAsync(
            7,
            new CreateMlflowExperimentRequest
            {
                Name = "churn-model",
                Tags = [new GitLabMlflowKeyValue { Key = "framework", Value = "sklearn" }],
                ArtifactLocation = "s3://ignored"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/create", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(
            """
            {"name":"churn-model","tags":[{"key":"framework","value":"sklearn"}],"artifact_location":"s3://ignored"}
            """,
            sentBody);

        Assert.Equal("12", created.ExperimentId);
    }

    [Fact]
    public async Task DeleteExperimentAsync_WrapsTheIdInMlflowsPostBody_RatherThanPuttingItInTheRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.DeleteExperimentAsync(7, "12", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/delete", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"experiment_id":"12"}""", sentBody);
    }

    [Fact]
    public async Task GetExperimentAsync_SendsTheIdAsAQueryParameter_AndDeserializesMlflowsEnvelope()
    {
        using StubHttpMessageHandler handler = new(_ => Json($$"""{ "experiment": {{ExperimentJson}} }"""));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowExperimentResponse response =
            await repository.GetExperimentAsync(7, "12", TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/get?experiment_id=12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(response.Experiment);
        Assert.Equal("12", response.Experiment!.ExperimentId);
        Assert.Equal("churn-model", response.Experiment.Name);
        Assert.Equal("active", response.Experiment.LifecycleStage);
        Assert.Equal("mlflow-artifacts:12", response.Experiment.ArtifactLocation);

        Assert.NotNull(response.Experiment.Tags);
        Assert.Equal(2, response.Experiment.Tags!.Count);
        Assert.Equal("framework", response.Experiment.Tags[0].Key);
        Assert.Equal("sklearn", response.Experiment.Tags[0].Value);

        // A tag GitLab returns with a null value must survive deserialization: only Key is non-nullable.
        Assert.Equal("owner", response.Experiment.Tags[1].Key);
        Assert.Null(response.Experiment.Tags[1].Value);
    }

    [Fact]
    public async Task GetExperimentByNameAsync_EncodesTheNamespacedProjectPath_AndTheExperimentName()
    {
        using StubHttpMessageHandler handler = new(_ => Json("""{ "experiment": null }"""));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowExperimentResponse response = await repository.GetExperimentByNameAsync(
            "group/subgroup/project", "churn model/v2", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject"
            + "/ml/mlflow/api/2.0/mlflow/experiments/get-by-name?experiment_name=churn%20model%2Fv2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Null(response.Experiment);
    }

    [Fact]
    public async Task ListExperimentsAsync_ReadsMlflowsCollectionEnvelope_RatherThanALinkHeaderPage()
    {
        using StubHttpMessageHandler handler = new(_ => Json($$"""{ "experiments": [ {{ExperimentJson}} ] }"""));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowExperimentList list =
            await repository.ListExperimentsAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/list", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(list.Experiments);
        GitLabMlflowExperiment experiment = Assert.Single(list.Experiments!);
        Assert.Equal("churn-model", experiment.Name);
    }

    [Fact]
    public async Task SearchExperimentsAsync_PostsThePagingBody_AndOmitsTheUnsetFilter()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json("""{ "experiments": [] }""", HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowExperimentList list = await repository.SearchExperimentsAsync(
            7,
            new SearchMlflowExperimentsRequest { MaxResults = 50, OrderBy = "name ASC", PageToken = "eyJpZCI6M30" },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/search", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"max_results":50,"order_by":"name ASC","page_token":"eyJpZCI6M30"}""", sentBody);

        Assert.NotNull(list.Experiments);
        Assert.Empty(list.Experiments!);
    }

    [Fact]
    public async Task SetExperimentTagAsync_PostsTheThreeFieldBody_AndExpectsNoResponseContent()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.SetExperimentTagAsync(
            7,
            new SetMlflowExperimentTagRequest { ExperimentId = "12", Key = "owner", Value = "ml-team" },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/experiments/set-experiment-tag",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"experiment_id":"12","key":"owner","value":"ml-team"}""", sentBody);
    }

    [Fact]
    public async Task GetMetricHistoryAsync_CombinesTheRequiredParametersWithTheGeneratedQueryProjection()
    {
        using StubHttpMessageHandler handler = new(_ => Json("""
                                                             {
                                                               "metrics": [
                                                                 { "key": "train/loss", "value": 0.5, "timestamp": 1717243800000, "step": 1 },
                                                                 { "key": "train/loss", "value": 0.25, "timestamp": 1717243850000, "step": 2 }
                                                               ],
                                                               "next_page_token": "eyJzdGVwIjozfQ"
                                                             }
                                                             """));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowMetricHistory history = await repository.GetMetricHistoryAsync(
            7,
            "a1b2c3d4e5f6",
            "train/loss",
            new MlflowMetricHistoryOptions { MaxResults = 500, PageToken = "eyJzdGVwIjoxfQ" },
            TestContext.Current.CancellationToken);

        // run_id and metric_key are one-off required parameters; max_results/page_token come from the
        // generated [GitLabQuery] projection. Both halves must land in one query string, and the metric
        // key's slash must be escaped.
        Assert.Equal(
            $"{MlflowPrefix}/mlflow/metrics/get-history"
            + "?run_id=a1b2c3d4e5f6&metric_key=train%2Floss&max_results=500&page_token=eyJzdGVwIjoxfQ",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("eyJzdGVwIjozfQ", history.NextPageToken);
        Assert.NotNull(history.Metrics);
        Assert.Equal(2, history.Metrics!.Count);
        Assert.Equal("train/loss", history.Metrics[0].Key);
        Assert.Equal(0.5, history.Metrics[0].Value);
        Assert.Equal(1717243800000L, history.Metrics[0].Timestamp);
        Assert.Equal(2L, history.Metrics[1].Step);
    }

    [Fact]
    public async Task GetMetricHistoryAsync_OmitsTheOptionalParameters_WhenNoOptionsAreSupplied()
    {
        using StubHttpMessageHandler handler = new(_ => Json("""{ "metrics": [], "next_page_token": null }"""));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.GetMetricHistoryAsync(7, "a1b2c3d4e5f6", "rmse",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/metrics/get-history?run_id=a1b2c3d4e5f6&metric_key=rmse",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateRunAsync_SendsTheExperimentIdAsAnInteger_AndDeserializesInfoAndData()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(RunJson, HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowRun run = await repository.CreateRunAsync(
            7,
            new CreateMlflowRunRequest
            {
                ExperimentId = 12,
                StartTime = 1717243800000,
                RunName = "candidate-4",
                Tags = [new GitLabMlflowKeyValue { Key = "stage", Value = "training" }]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/create", handler.LastRequest?.RequestUri?.AbsoluteUri);

        // runs/create is the one operation whose experiment_id is an integer on the wire; every other
        // operation, and every response, carries it as a string.
        Assert.Equal(
            """
            {"experiment_id":12,"start_time":1717243800000,"tags":[{"key":"stage","value":"training"}],"run_name":"candidate-4"}
            """,
            sentBody);

        Assert.NotNull(run.Info);
        Assert.Equal("a1b2c3d4e5f6", run.Info!.RunId);
        Assert.Equal("12", run.Info.ExperimentId);
        Assert.Equal("FINISHED", run.Info.Status);
        Assert.Equal("mlflow-artifacts:12", run.Info.ArtifactUri);
        Assert.Equal(1717243800000L, run.Info.StartTime);

        Assert.NotNull(run.Data);
        Assert.NotNull(run.Data!.Metrics);
        GitLabMlflowMetric metric = Assert.Single(run.Data.Metrics!);
        Assert.Equal("rmse", metric.Key);
        Assert.Equal(0.25, metric.Value);
        Assert.Equal(3L, metric.Step);

        Assert.NotNull(run.Data.Params);
        GitLabMlflowKeyValue parameter = Assert.Single(run.Data.Params!);
        Assert.Equal("n_estimators", parameter.Key);
        Assert.Equal("100", parameter.Value);

        Assert.NotNull(run.Data.Tags);
        Assert.Equal("stage", Assert.Single(run.Data.Tags!).Key);
    }

    [Fact]
    public async Task GetRunAsync_SendsOnlyTheRunId_BecauseGitLabDocumentsRunUuidAsIgnored()
    {
        using StubHttpMessageHandler handler = new(_ => Json(RunJson));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowRun run = await repository.GetRunAsync(7, "a1b2c3d4e5f6", TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/get?run_id=a1b2c3d4e5f6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("candidate-4", run.Info?.RunName);
    }

    [Fact]
    public async Task SearchRunsAsync_SendsTheExperimentIdsArray()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(RunJson, HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.SearchRunsAsync(
            7,
            new SearchMlflowRunsRequest { ExperimentIds = ["12"], MaxResults = 100, OrderBy = "metrics.rmse DESC" },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/search", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"experiment_ids":["12"],"max_results":100,"order_by":"metrics.rmse DESC"}""", sentBody);
    }

    [Fact]
    public async Task UpdateRunAsync_SendsMlflowsUppercaseStatus_AndReadsTheRunInfoEnvelope()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json("""{ "run_info": { "run_id": "a1b2c3d4e5f6", "status": "FINISHED" } }""",
                HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowUpdateRunResponse response = await repository.UpdateRunAsync(
            7,
            new UpdateMlflowRunRequest
            {
                RunId = "a1b2c3d4e5f6", Status = GitLabMlflowRunStatus.Finished, EndTime = 1717243900000
            },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/update", handler.LastRequest?.RequestUri?.AbsoluteUri);

        // MLflow's status vocabulary is SCREAMING_CASE, which no naming policy would produce - every
        // member carries an explicit [JsonStringEnumMemberName].
        Assert.Equal("""{"run_id":"a1b2c3d4e5f6","status":"FINISHED","end_time":1717243900000}""", sentBody);

        // The response names the member "run_info", not "info" as a full run does.
        Assert.Equal("FINISHED", response.RunInfo?.Status);
    }

    [Fact]
    public async Task DeleteRunAsync_WrapsTheRunIdInMlflowsPostBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.DeleteRunAsync(7, "a1b2c3d4e5f6", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"{MlflowPrefix}/mlflow/runs/delete", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"run_id":"a1b2c3d4e5f6"}""", sentBody);
    }

    [Fact]
    public async Task LogMetricAsync_PostsTheSample()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.LogMetricAsync(
            7,
            new LogMlflowMetricRequest
            {
                RunId = "a1b2c3d4e5f6",
                Key = "rmse",
                Value = 0.25,
                Timestamp = 1717243850000,
                Step = 3
            },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/log-metric", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"run_id":"a1b2c3d4e5f6","key":"rmse","value":0.25,"timestamp":1717243850000,"step":3}""",
            sentBody);
    }

    [Fact]
    public async Task LogParameterAsync_PostsTheParameter()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.LogParameterAsync(
            7,
            new LogMlflowParameterRequest { RunId = "a1b2c3d4e5f6", Key = "n_estimators", Value = "100" },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/log-parameter", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"run_id":"a1b2c3d4e5f6","key":"n_estimators","value":"100"}""", sentBody);
    }

    [Fact]
    public async Task SetRunTagAsync_PostsTheTag()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.SetRunTagAsync(
            7,
            new SetMlflowRunTagRequest { RunId = "a1b2c3d4e5f6", Key = "stage", Value = "training" },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/set-tag", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"run_id":"a1b2c3d4e5f6","key":"stage","value":"training"}""", sentBody);
    }

    [Fact]
    public async Task LogBatchAsync_SendsMetricsAndParamsAsSeparateArraysOfTheirOwnRecordTypes()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.LogBatchAsync(
            7,
            new LogMlflowBatchRequest
            {
                RunId = "a1b2c3d4e5f6",
                Metrics =
                [
                    new MlflowMetricEntry { Key = "rmse", Value = 0.5, Timestamp = 1717243800000, Step = 1 },
                    new MlflowMetricEntry { Key = "rmse", Value = 0.25, Timestamp = 1717243850000 }
                ],
                Params = [new MlflowParameterEntry { Key = "n_estimators", Value = "100" }]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/runs/log-batch", handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The second metric omits "step" entirely rather than sending null.
        Assert.Equal(
            """
            {"run_id":"a1b2c3d4e5f6","metrics":[{"key":"rmse","value":0.5,"timestamp":1717243800000,"step":1},{"key":"rmse","value":0.25,"timestamp":1717243850000}],"params":[{"key":"n_estimators","value":"100"}]}
            """,
            sentBody);
    }

    [Fact]
    public async Task CreateModelVersionAsync_PostsTheBody_AndDeserializesTheFullModelVersion()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(ModelVersionJson, HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowModelVersion version = await repository.CreateModelVersionAsync(
            7,
            new CreateMlflowModelVersionRequest
            {
                Name = "churn-model", Description = "promoted from candidate-4", RunId = "a1b2c3d4e5f6"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/model-versions/create", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"name":"churn-model","description":"promoted from candidate-4","run_id":"a1b2c3d4e5f6"}""",
            sentBody);

        Assert.Equal("churn-model", version.Name);
        Assert.Equal("3", version.Version);
        Assert.Equal("READY", version.Status);
        Assert.Equal("development", version.CurrentStage);
        Assert.Equal(1717243800000L, version.CreationTimestamp);

        Assert.NotNull(version.Tags);
        Assert.Equal("a100", Assert.Single(version.Tags!).Value);

        Assert.NotNull(version.Aliases);
        Assert.Equal("champion", Assert.Single(version.Aliases!));
    }

    [Fact]
    public async Task GetModelVersionAsync_SendsNameAndVersionAsQueryParameters()
    {
        using StubHttpMessageHandler handler = new(_ => Json(ModelVersionJson));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowModelVersion version =
            await repository.GetModelVersionAsync(7, "churn model", "3", TestContext.Current.CancellationToken);

        Assert.Equal($"{MlflowPrefix}/mlflow/model-versions/get?name=churn%20model&version=3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("3", version.Version);
    }

    [Fact]
    public async Task GetModelVersionDownloadUriAsync_SendsTheVersionAsAnInteger()
    {
        using StubHttpMessageHandler handler = new(_ => Json("""{ "artifact_uri": "mlflow-artifacts:3" }"""));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabMlflowArtifactUri download = await repository.GetModelVersionDownloadUriAsync(
            7, "churn-model", 3, TestContext.Current.CancellationToken);

        // The spec types this endpoint's "version" as an integer while model-versions/get types it as a
        // string; both are honoured rather than normalised away.
        Assert.Equal($"{MlflowPrefix}/mlflow/model-versions/get-download-uri?name=churn-model&version=3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("mlflow-artifacts:3", download.ArtifactUri);
    }

    [Fact]
    public async Task UpdateModelVersionAsync_UsesPatch_WhichIsTheOnlyPatchInThisArea()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(ModelVersionJson);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        await repository.UpdateModelVersionAsync(
            7,
            new UpdateMlflowModelVersionRequest { Name = "churn-model", Description = "new description" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal($"{MlflowPrefix}/mlflow/model-versions/update", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"churn-model","description":"new description"}""", sentBody);
    }

    [Fact]
    public async Task GetExperimentAsync_MapsA404ToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler =
            new(_ => Json("""{ "message": "404 Experiment Not Found" }""", HttpStatusCode.NotFound));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        MlExperimentsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetExperimentAsync(7, "999", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}