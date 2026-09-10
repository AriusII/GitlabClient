using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class PipelineSchedulesClient(IGitLabApiConnection connection) : IPipelineSchedulesClient
{
    public IAsyncEnumerable<GitLabPipelineSchedule> ListAsync(ProjectId projectId,
        PipelineScheduleListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineScheduleArray,
            cancellationToken);
    }

    public Task<GitLabPipelineSchedule> GetAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineSchedule,
            cancellationToken);
    }

    public Task<GitLabPipelineSchedule> CreateAsync(ProjectId projectId, CreatePipelineScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Build(),
            request,
            GitLabJsonContext.Default.CreatePipelineScheduleRequest,
            GitLabJsonContext.Default.GitLabPipelineSchedule,
            cancellationToken);
    }

    public Task<GitLabPipelineSchedule> UpdateAsync(ProjectId projectId, long pipelineScheduleId,
        UpdatePipelineScheduleRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdatePipelineScheduleRequest,
            GitLabJsonContext.Default.GitLabPipelineSchedule,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Uses the body-less <c>PostAsync</c> overload on purpose: GitLab answers this endpoint with
    ///     <c>201</c> and a bare acknowledgement message rather than with the schedule or the pipeline it
    ///     created, so deserializing the response would fail on every successful call.
    /// </summary>
    public Task PlayAsync(ProjectId projectId, long pipelineScheduleId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Literal("play")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabPipelineSchedule> TakeOwnershipAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Literal("take_ownership")
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineSchedule,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long pipelineScheduleId,
        PipelineScheduleRunListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Literal("pipelines")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPipelineArray,
            cancellationToken);
    }

    public Task<GitLabVariable> CreateVariableAsync(ProjectId projectId, long pipelineScheduleId,
        CreatePipelineScheduleVariableRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipeline_schedules")
                .Segment(pipelineScheduleId)
                .Literal("variables")
                .Build(),
            request,
            GitLabJsonContext.Default.CreatePipelineScheduleVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> GetVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            VariableRoute(projectId, pipelineScheduleId, key),
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> UpdateVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        UpdatePipelineScheduleVariableRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            VariableRoute(projectId, pipelineScheduleId, key),
            request,
            GitLabJsonContext.Default.UpdatePipelineScheduleVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task DeleteVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            VariableRoute(projectId, pipelineScheduleId, key),
            cancellationToken);
    }

    private static Uri VariableRoute(ProjectId projectId, long pipelineScheduleId, string key)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("pipeline_schedules")
            .Segment(pipelineScheduleId)
            .Literal("variables")
            .Escaped(key)
            .Build();
    }
}