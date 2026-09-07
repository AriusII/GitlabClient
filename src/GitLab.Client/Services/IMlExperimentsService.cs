using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the MLflow-compatible experiment tracking area, sitting between
///     the public <c>IMlExperimentsClient</c> controller and <c>IMlExperimentsRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is
///     the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IMlExperimentsService
{
    Task<GitLabFileResponse> DownloadArtifactAsync(ProjectId projectId, string? path = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadArtifactFileAsync(ProjectId projectId, string modelVersion, string filePath,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowNewExperiment> CreateExperimentAsync(ProjectId projectId,
        CreateMlflowExperimentRequest request, CancellationToken cancellationToken = default);

    Task DeleteExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowExperimentResponse> GetExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowExperimentResponse> GetExperimentByNameAsync(ProjectId projectId, string experimentName,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowExperimentList> ListExperimentsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowExperimentList> SearchExperimentsAsync(ProjectId projectId,
        SearchMlflowExperimentsRequest request, CancellationToken cancellationToken = default);

    Task SetExperimentTagAsync(ProjectId projectId, SetMlflowExperimentTagRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowMetricHistory> GetMetricHistoryAsync(ProjectId projectId, string runId, string metricKey,
        MlflowMetricHistoryOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabMlflowModelVersion> CreateModelVersionAsync(ProjectId projectId,
        CreateMlflowModelVersionRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMlflowModelVersion> GetModelVersionAsync(ProjectId projectId, string name, string version,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowArtifactUri> GetModelVersionDownloadUriAsync(ProjectId projectId, string name, long version,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowModelVersion> UpdateModelVersionAsync(ProjectId projectId,
        UpdateMlflowModelVersionRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMlflowRun> CreateRunAsync(ProjectId projectId, CreateMlflowRunRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowRun> GetRunAsync(ProjectId projectId, string runId,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowRun> SearchRunsAsync(ProjectId projectId, SearchMlflowRunsRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMlflowUpdateRunResponse> UpdateRunAsync(ProjectId projectId, UpdateMlflowRunRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteRunAsync(ProjectId projectId, string runId, CancellationToken cancellationToken = default);

    Task LogMetricAsync(ProjectId projectId, LogMlflowMetricRequest request,
        CancellationToken cancellationToken = default);

    Task LogParameterAsync(ProjectId projectId, LogMlflowParameterRequest request,
        CancellationToken cancellationToken = default);

    Task LogBatchAsync(ProjectId projectId, LogMlflowBatchRequest request,
        CancellationToken cancellationToken = default);

    Task SetRunTagAsync(ProjectId projectId, SetMlflowRunTagRequest request,
        CancellationToken cancellationToken = default);
}