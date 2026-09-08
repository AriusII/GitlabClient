using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Pipelines resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPipelinesService), typeof(IPipelinesClient))]
internal interface IPipelinesRepository
{
    /// <summary>Streams a project's pipelines (<c>GET /projects/:id/pipelines</c>), following the pagination links.</summary>
    IAsyncEnumerable<GitLabPipeline> ListAsync(ProjectId projectId, PipelineListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one pipeline by its ID (<c>GET /projects/:id/pipelines/:pipeline_id</c>).</summary>
    Task<GitLabPipeline> GetAsync(ProjectId projectId, long pipelineId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates (triggers) a new pipeline for a ref (<c>POST /projects/:id/pipeline</c> - note the
    ///     singular route, unlike every other pipeline endpoint).
    /// </summary>
    Task<GitLabPipeline> CreateAsync(ProjectId projectId, CreatePipelineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels all of a pipeline's running jobs (<c>POST /projects/:id/pipelines/:pipeline_id/cancel</c>).</summary>
    Task<GitLabPipeline> CancelAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    /// <summary>Retries a pipeline's failed and canceled jobs (<c>POST /projects/:id/pipelines/:pipeline_id/retry</c>).</summary>
    Task<GitLabPipeline> RetryAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a pipeline along with its jobs, logs, artifacts and triggers
    ///     (<c>DELETE /projects/:id/pipelines/:pipeline_id</c>). Requires the Owner role; irreversible.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, long pipelineId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the pipelines the authenticated user triggered, across every project they can see
    ///     (<c>GET /pipelines</c>), streaming every page.
    /// </summary>
    IAsyncEnumerable<GitLabPipeline> ListForCurrentUserAsync(UserPipelineListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the latest pipeline for a ref (<c>GET /projects/:id/pipelines/latest</c>), falling back
    ///     to the project's default branch when <paramref name="refName" /> is omitted.
    /// </summary>
    Task<GitLabPipeline> GetLatestAsync(ProjectId projectId, string? refName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renames a pipeline (<c>PUT /projects/:id/pipelines/:pipeline_id/metadata</c>) and returns it
    ///     carrying the new <see cref="GitLabPipeline.Name" />.
    /// </summary>
    Task<GitLabPipeline> UpdateMetadataAsync(ProjectId projectId, long pipelineId,
        UpdatePipelineMetadataRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the pipeline's full test report, including every individual case. Prefer
    ///     <see cref="GetTestReportSummaryAsync" /> for a pipeline with a large report.
    /// </summary>
    Task<GitLabTestReport> GetTestReportAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves the pipeline's test report rolled up per suite, without the individual cases.</summary>
    Task<GitLabTestReportSummary> GetTestReportSummaryAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the pipeline's trigger (bridge) jobs - the jobs that start a downstream pipeline - streaming
    ///     every page. Each carries <see cref="GitLabJob.DownstreamPipeline" />. This is the endpoint that
    ///     supersedes the deprecated <c>bridges</c> one.
    /// </summary>
    IAsyncEnumerable<GitLabJob> ListTriggerJobsAsync(ProjectId projectId, long pipelineId,
        TriggerJobListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the CI/CD variables the pipeline ran with, streaming every page. Requires at least the
    ///     Developer role; GitLab answers 403 otherwise.
    /// </summary>
    IAsyncEnumerable<GitLabVariable> ListVariablesAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);
}