using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's MLflow-compatible experiment tracking API
///     (<c>/projects/:id/ml/mlflow/api/2.0/...</c>) - experiments, runs, metrics, parameters, model
///     versions and artifacts.
///     <para>
///         This area deliberately speaks MLflow's dialect rather than GitLab's. An MLflow <em>run</em> is
///         a GitLab <em>candidate</em>; experiment and run identifiers are project-relative strings, not
///         global IDs; timestamps are Unix milliseconds as bare integers rather than ISO 8601; several
///         responses are envelopes (<c>{ "experiment": ... }</c>) because that is MLflow's message shape.
///         The types below preserve those shapes so a payload captured from an MLflow client round-trips
///         unchanged.
///     </para>
///     <para>
///         Pagination here is MLflow's <c>page_token</c> in the body, not GitLab's RFC 5988 <c>Link</c>
///         header, so the list-shaped calls return one envelope at a time rather than an
///         <see cref="IAsyncEnumerable{T}" />.
///     </para>
///     <para>
///         The MLflow model <em>registry</em> half of GitLab's MLOps API
///         (<c>/projects/:id/ml/models</c> and <c>.../mlflow/registered-models</c>) is a separate resource;
///         only the model-<em>version</em> endpoints of the MLflow API live here.
///     </para>
/// </summary>
public interface IMlExperimentsClient
{
    /// <summary>
    ///     Downloads an MLflow artifact by its artifact path - a model version ID, optionally followed by a
    ///     path within it (<c>15/MLmodel</c>). The body is streamed and never reaches the JSON serializer.
    /// </summary>
    /// <param name="projectId">The project, by numeric ID or namespaced path.</param>
    /// <param name="path">The artifact path. Omitted entirely when <see langword="null" /> or blank.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must dispose it - prefer <c>await using</c>.
    /// </returns>
    Task<GitLabFileResponse> DownloadArtifactAsync(ProjectId projectId, string? path = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file from a model version's artifacts. Both segments are caller-supplied free text
    ///     and are percent-encoded, so a <paramref name="filePath" /> containing <c>/</c> stays a single
    ///     path segment - pass it raw.
    /// </summary>
    /// <param name="projectId">The project, by numeric ID or namespaced path.</param>
    /// <param name="modelVersion">The model version the artifact belongs to.</param>
    /// <param name="filePath">The file within the version's artifacts.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>The open body. The caller owns it and must dispose it - prefer <c>await using</c>.</returns>
    Task<GitLabFileResponse> DownloadArtifactFileAsync(ProjectId projectId, string modelVersion, string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an experiment. The response carries only the new ID; read the rest back with
    ///     <see cref="GetExperimentAsync" />.
    /// </summary>
    Task<GitLabMlflowNewExperiment> CreateExperimentAsync(ProjectId projectId,
        CreateMlflowExperimentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an experiment. MLflow models this as a <c>POST</c> carrying the ID in the body rather
    ///     than as a <c>DELETE</c>, which is why the ID is not in the route.
    /// </summary>
    Task DeleteExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one experiment by its project-relative ID.</summary>
    Task<GitLabMlflowExperimentResponse> GetExperimentAsync(ProjectId projectId, string experimentId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one experiment by name. Experiment names are unique within a project.</summary>
    Task<GitLabMlflowExperimentResponse> GetExperimentByNameAsync(ProjectId projectId, string experimentName,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the project's experiments. Use <see cref="SearchExperimentsAsync" /> to page or order them.</summary>
    Task<GitLabMlflowExperimentList> ListExperimentsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The paged, ordered form of <see cref="ListExperimentsAsync" />. GitLab accepts MLflow's
    ///     <c>filter</c> expression and ignores it.
    /// </summary>
    Task<GitLabMlflowExperimentList> SearchExperimentsAsync(ProjectId projectId,
        SearchMlflowExperimentsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sets one tag on an experiment, replacing any existing tag with the same key.</summary>
    Task SetExperimentTagAsync(ProjectId projectId, SetMlflowExperimentTagRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one page of a metric's full recorded history for a run - every sample, not just the latest
    ///     value that <see cref="GitLabMlflowRunData.Metrics" /> carries. Walk further pages by feeding
    ///     <see cref="GitLabMlflowMetricHistory.NextPageToken" /> back through
    ///     <see cref="MlflowMetricHistoryOptions.PageToken" />.
    /// </summary>
    /// <param name="projectId">The project, by numeric ID or namespaced path.</param>
    /// <param name="runId">The candidate's UUID.</param>
    /// <param name="metricKey">The metric name.</param>
    /// <param name="options">Page size and continuation token. Optional.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabMlflowMetricHistory> GetMetricHistoryAsync(ProjectId projectId, string runId, string metricKey,
        MlflowMetricHistoryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a model version, optionally promoting a candidate to it.</summary>
    Task<GitLabMlflowModelVersion> CreateModelVersionAsync(ProjectId projectId,
        CreateMlflowModelVersionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets a model version by registered model name and version.</summary>
    Task<GitLabMlflowModelVersion> GetModelVersionAsync(ProjectId projectId, string name, string version,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets where a model version's artifacts can be downloaded from, in MLflow's own
    ///     <c>mlflow-artifacts:&lt;version&gt;</c> form.
    /// </summary>
    /// <remarks>
    ///     The <c>version</c> is a <see cref="long" /> here and a <see cref="string" /> on
    ///     <see cref="GetModelVersionAsync" />: the spec genuinely types the two parameters differently.
    /// </remarks>
    Task<GitLabMlflowArtifactUri> GetModelVersionDownloadUriAsync(ProjectId projectId, string name, long version,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a model version's description. The only <c>PATCH</c> in this area, and the only mutation
    ///     that names its target entirely through the body.
    /// </summary>
    Task<GitLabMlflowModelVersion> UpdateModelVersionAsync(ProjectId projectId,
        UpdateMlflowModelVersionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Creates a run - a candidate - under an experiment.</summary>
    Task<GitLabMlflowRun> CreateRunAsync(ProjectId projectId, CreateMlflowRunRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a run by its UUID.
    /// </summary>
    /// <remarks>
    ///     GitLab also declares a <c>run_uuid</c> query parameter on this endpoint and documents it as
    ///     ignored, so it is not exposed here.
    /// </remarks>
    Task<GitLabMlflowRun> GetRunAsync(ProjectId projectId, string runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches the runs of an experiment.
    /// </summary>
    /// <remarks>
    ///     The spec types the response as a single run rather than as a page of them, and this wrapper
    ///     follows the spec. If a GitLab instance answers with a collection envelope instead, reach for
    ///     <see cref="IGitLabApiConnection" /> directly until the pinned spec is corrected.
    /// </remarks>
    Task<GitLabMlflowRun> SearchRunsAsync(ProjectId projectId, SearchMlflowRunsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a run's status and end time. This is the one place the status vocabulary is enumerated -
    ///     see <see cref="GitLabMlflowRunStatus" />.
    /// </summary>
    Task<GitLabMlflowUpdateRunResponse> UpdateRunAsync(ProjectId projectId, UpdateMlflowRunRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a run. Like the experiment delete, MLflow models this as a <c>POST</c> carrying the ID in
    ///     the body.
    /// </summary>
    Task DeleteRunAsync(ProjectId projectId, string runId, CancellationToken cancellationToken = default);

    /// <summary>Logs one metric sample against a run.</summary>
    Task LogMetricAsync(ProjectId projectId, LogMlflowMetricRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Logs one parameter against a run.</summary>
    Task LogParameterAsync(ProjectId projectId, LogMlflowParameterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Logs many metrics and parameters in one call - how an MLflow client flushes a training loop.
    ///     Prefer this over repeated <see cref="LogMetricAsync" /> calls.
    /// </summary>
    Task LogBatchAsync(ProjectId projectId, LogMlflowBatchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Sets one tag on a run, replacing any existing tag with the same key.</summary>
    Task SetRunTagAsync(ProjectId projectId, SetMlflowRunTagRequest request,
        CancellationToken cancellationToken = default);
}