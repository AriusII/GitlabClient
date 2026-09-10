using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class MlExperimentsClient(IGitLabApiConnection connection) : IMlExperimentsClient
{
    public Task<GitLabFileResponse> DownloadArtifactAsync(ProjectId projectId, string? path = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ArtifactsRoute(projectId).Query("path", path).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadArtifactFileAsync(ProjectId projectId, string modelVersion,
        string filePath, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ArtifactsRoute(projectId).Escaped(modelVersion).Escaped(filePath).Build(),
            cancellationToken);
    }

    public Task<GitLabMlflowNewExperiment> CreateExperimentAsync(ProjectId projectId,
        CreateMlflowExperimentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ExperimentsRoute(projectId).Literal("create").Build(),
            request,
            GitLabJsonContext.Default.CreateMlflowExperimentRequest,
            GitLabJsonContext.Default.GitLabMlflowNewExperiment,
            cancellationToken);
    }

    public Task DeleteExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ExperimentsRoute(projectId).Literal("delete").Build(),
            new DeleteMlflowExperimentRequest { ExperimentId = experimentId },
            GitLabJsonContext.Default.DeleteMlflowExperimentRequest,
            cancellationToken);
    }

    public Task<GitLabMlflowExperimentResponse> GetExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ExperimentsRoute(projectId).Literal("get").Query("experiment_id", experimentId).Build(),
            GitLabJsonContext.Default.GitLabMlflowExperimentResponse,
            cancellationToken);
    }

    public Task<GitLabMlflowExperimentResponse> GetExperimentByNameAsync(ProjectId projectId, string experimentName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ExperimentsRoute(projectId).Literal("get-by-name").Query("experiment_name", experimentName).Build(),
            GitLabJsonContext.Default.GitLabMlflowExperimentResponse,
            cancellationToken);
    }

    public Task<GitLabMlflowExperimentList> ListExperimentsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ExperimentsRoute(projectId).Literal("list").Build(),
            GitLabJsonContext.Default.GitLabMlflowExperimentList,
            cancellationToken);
    }

    public Task<GitLabMlflowExperimentList> SearchExperimentsAsync(ProjectId projectId,
        SearchMlflowExperimentsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ExperimentsRoute(projectId).Literal("search").Build(),
            request,
            GitLabJsonContext.Default.SearchMlflowExperimentsRequest,
            GitLabJsonContext.Default.GitLabMlflowExperimentList,
            cancellationToken);
    }

    public Task SetExperimentTagAsync(ProjectId projectId, SetMlflowExperimentTagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ExperimentsRoute(projectId).Literal("set-experiment-tag").Build(),
            request,
            GitLabJsonContext.Default.SetMlflowExperimentTagRequest,
            cancellationToken);
    }

    public Task<GitLabMlflowMetricHistory> GetMetricHistoryAsync(ProjectId projectId, string runId, string metricKey,
        MlflowMetricHistoryOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MetricsRoute(projectId).Literal("get-history")
                .Query("run_id", runId).Query("metric_key", metricKey).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabMlflowMetricHistory,
            cancellationToken);
    }

    public Task<GitLabMlflowModelVersion> CreateModelVersionAsync(ProjectId projectId,
        CreateMlflowModelVersionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ModelVersionsRoute(projectId).Literal("create").Build(),
            request,
            GitLabJsonContext.Default.CreateMlflowModelVersionRequest,
            GitLabJsonContext.Default.GitLabMlflowModelVersion,
            cancellationToken);
    }

    public Task<GitLabMlflowModelVersion> GetModelVersionAsync(ProjectId projectId, string name, string version,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ModelVersionsRoute(projectId).Literal("get").Query("name", name).Query("version", version).Build(),
            GitLabJsonContext.Default.GitLabMlflowModelVersion,
            cancellationToken);
    }

    public Task<GitLabMlflowArtifactUri> GetModelVersionDownloadUriAsync(ProjectId projectId, string name,
        long version, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ModelVersionsRoute(projectId).Literal("get-download-uri").Query("name", name).Query("version", version)
                .Build(),
            GitLabJsonContext.Default.GitLabMlflowArtifactUri,
            cancellationToken);
    }

    public Task<GitLabMlflowModelVersion> UpdateModelVersionAsync(ProjectId projectId,
        UpdateMlflowModelVersionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            ModelVersionsRoute(projectId).Literal("update").Build(),
            request,
            GitLabJsonContext.Default.UpdateMlflowModelVersionRequest,
            GitLabJsonContext.Default.GitLabMlflowModelVersion,
            cancellationToken);
    }

    public Task<GitLabMlflowRun> CreateRunAsync(ProjectId projectId, CreateMlflowRunRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("create").Build(),
            request,
            GitLabJsonContext.Default.CreateMlflowRunRequest,
            GitLabJsonContext.Default.GitLabMlflowRun,
            cancellationToken);
    }

    public Task<GitLabMlflowRun> GetRunAsync(ProjectId projectId, string runId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RunsRoute(projectId).Literal("get").Query("run_id", runId).Build(),
            GitLabJsonContext.Default.GitLabMlflowRun,
            cancellationToken);
    }

    public Task<GitLabMlflowRun> SearchRunsAsync(ProjectId projectId, SearchMlflowRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("search").Build(),
            request,
            GitLabJsonContext.Default.SearchMlflowRunsRequest,
            GitLabJsonContext.Default.GitLabMlflowRun,
            cancellationToken);
    }

    public Task<GitLabMlflowUpdateRunResponse> UpdateRunAsync(ProjectId projectId, UpdateMlflowRunRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("update").Build(),
            request,
            GitLabJsonContext.Default.UpdateMlflowRunRequest,
            GitLabJsonContext.Default.GitLabMlflowUpdateRunResponse,
            cancellationToken);
    }

    public Task DeleteRunAsync(ProjectId projectId, string runId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("delete").Build(),
            new DeleteMlflowRunRequest { RunId = runId },
            GitLabJsonContext.Default.DeleteMlflowRunRequest,
            cancellationToken);
    }

    public Task LogMetricAsync(ProjectId projectId, LogMlflowMetricRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("log-metric").Build(),
            request,
            GitLabJsonContext.Default.LogMlflowMetricRequest,
            cancellationToken);
    }

    public Task LogParameterAsync(ProjectId projectId, LogMlflowParameterRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("log-parameter").Build(),
            request,
            GitLabJsonContext.Default.LogMlflowParameterRequest,
            cancellationToken);
    }

    public Task LogBatchAsync(ProjectId projectId, LogMlflowBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("log-batch").Build(),
            request,
            GitLabJsonContext.Default.LogMlflowBatchRequest,
            cancellationToken);
    }

    public Task SetRunTagAsync(ProjectId projectId, SetMlflowRunTagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RunsRoute(projectId).Literal("set-tag").Build(),
            request,
            GitLabJsonContext.Default.SetMlflowRunTagRequest,
            cancellationToken);
    }

    /// <summary>
    ///     The shared prefix of every MLflow-compatible route: GitLab mounts MLflow's own API under the
    ///     project, versioned by MLflow's <c>2.0</c> rather than by GitLab's <c>v4</c>. Every word here comes
    ///     from the route template, so all of them are <c>Literal</c>.
    /// </summary>
    private static GitLabRouteBuilder MlflowRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId)
            .Literal("ml").Literal("mlflow").Literal("api").Literal("2.0");
    }

    /// <summary>
    ///     The artifact routes sit beside the MLflow API rather than under it - <c>mlflow-artifacts</c>, with
    ///     a hyphen, is a sibling namespace of <c>mlflow</c>, not a member of it.
    /// </summary>
    private static GitLabRouteBuilder ArtifactsRoute(ProjectId projectId)
    {
        return MlflowRoute(projectId).Literal("mlflow-artifacts").Literal("artifacts");
    }

    private static GitLabRouteBuilder ExperimentsRoute(ProjectId projectId)
    {
        return MlflowRoute(projectId).Literal("mlflow").Literal("experiments");
    }

    private static GitLabRouteBuilder RunsRoute(ProjectId projectId)
    {
        return MlflowRoute(projectId).Literal("mlflow").Literal("runs");
    }

    private static GitLabRouteBuilder ModelVersionsRoute(ProjectId projectId)
    {
        return MlflowRoute(projectId).Literal("mlflow").Literal("model-versions");
    }

    private static GitLabRouteBuilder MetricsRoute(ProjectId projectId)
    {
        return MlflowRoute(projectId).Literal("mlflow").Literal("metrics");
    }
}