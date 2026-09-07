using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Pipelines, sitting between the public <c>IPipelinesClient</c>
///     controller and <c>IPipelinesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IPipelinesService
{
    IAsyncEnumerable<GitLabPipeline> ListAsync(ProjectId projectId, PipelineListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPipeline> GetAsync(ProjectId projectId, long pipelineId, CancellationToken cancellationToken = default);

    Task<GitLabPipeline> CreateAsync(ProjectId projectId, CreatePipelineRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPipeline> CancelAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    Task<GitLabPipeline> RetryAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

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