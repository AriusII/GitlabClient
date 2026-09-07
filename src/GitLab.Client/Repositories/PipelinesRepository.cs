using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PipelinesRepository(IGitLabApiConnection connection) : IPipelinesRepository
{
    public IAsyncEnumerable<GitLabPipeline> ListAsync(ProjectId projectId, PipelineListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineArray,
            cancellationToken);
    }

    public Task<GitLabPipeline> GetAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipelines").Segment(pipelineId).Build(),
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabPipeline> CreateAsync(ProjectId projectId, CreatePipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipeline").Build(),
            request,
            GitLabJsonContext.Default.CreatePipelineRequest,
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabPipeline> CancelAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipelines").Segment(pipelineId)
                .Literal("cancel").Build(),
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabPipeline> RetryAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipelines").Segment(pipelineId)
                .Literal("retry").Build(),
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long pipelineId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipelines").Segment(pipelineId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPipeline> ListForCurrentUserAsync(UserPipelineListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("pipelines")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineArray,
            cancellationToken);
    }

    public Task<GitLabPipeline> GetLatestAsync(ProjectId projectId, string? refName = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Literal("latest")
                .Query("ref", refName)
                .Build(),
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabPipeline> UpdateMetadataAsync(ProjectId projectId, long pipelineId,
        UpdatePipelineMetadataRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("metadata")
                .Build(),
            request,
            GitLabJsonContext.Default.UpdatePipelineMetadataRequest,
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabTestReport> GetTestReportAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("test_report")
                .Build(),
            GitLabJsonContext.Default.GitLabTestReport,
            cancellationToken);
    }

    public Task<GitLabTestReportSummary> GetTestReportSummaryAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("test_report_summary")
                .Build(),
            GitLabJsonContext.Default.GitLabTestReportSummary,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJob> ListTriggerJobsAsync(ProjectId projectId, long pipelineId,
        TriggerJobListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("trigger_jobs")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabJobArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVariable> ListVariablesAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("variables")
                .Build(),
            GitLabJsonContext.Default.GitLabVariableArray,
            cancellationToken);
    }
}