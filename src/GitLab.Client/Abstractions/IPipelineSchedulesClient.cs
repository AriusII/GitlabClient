using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Pipeline schedules" API area (<c>/projects/:id/pipeline_schedules</c>) - the
///     scheduled-pipeline CRUD plus its two action endpoints, <c>play</c> and <c>take_ownership</c>.
/// </summary>
public interface IPipelineSchedulesClient
{
    /// <summary>Lists a project's pipeline schedules, streaming every page.</summary>
    IAsyncEnumerable<GitLabPipelineSchedule> ListAsync(ProjectId projectId,
        PipelineScheduleListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one pipeline schedule, including the pipeline it last triggered.</summary>
    Task<GitLabPipelineSchedule> GetAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a pipeline schedule for a project, armed and ready to fire on its cron.</summary>
    Task<GitLabPipelineSchedule> CreateAsync(ProjectId projectId, CreatePipelineScheduleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a pipeline schedule; GitLab recomputes the next run time after the change.</summary>
    Task<GitLabPipelineSchedule> UpdateAsync(ProjectId projectId, long pipelineScheduleId,
        UpdatePipelineScheduleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a pipeline schedule.</summary>
    Task DeleteAsync(ProjectId projectId, long pipelineScheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Triggers the schedule immediately, out of band of its cron. GitLab acknowledges with a bare
    ///     message rather than with the pipeline it created, so nothing is returned - read
    ///     <see cref="ListPipelinesAsync" /> for the resulting run.
    /// </summary>
    Task PlayAsync(ProjectId projectId, long pipelineScheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Makes the calling user the schedule's owner, so future runs use that user's permissions. Returns
    ///     the schedule carrying its new <see cref="GitLabPipelineSchedule.Owner" />.
    /// </summary>
    Task<GitLabPipelineSchedule> TakeOwnershipAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the pipelines this schedule has triggered, streaming every page.</summary>
    IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long pipelineScheduleId,
        PipelineScheduleRunListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a CI/CD variable to a pipeline schedule. Only pipelines this schedule triggers see it - it is
    ///     not a project variable.
    /// </summary>
    Task<GitLabVariable> CreateVariableAsync(ProjectId projectId, long pipelineScheduleId,
        CreatePipelineScheduleVariableRequest request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one of a pipeline schedule's variables by key.</summary>
    Task<GitLabVariable> GetVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a pipeline schedule variable's value or type. The key is part of the route and cannot be
    ///     changed; delete the variable and create a new one instead.
    /// </summary>
    Task<GitLabVariable> UpdateVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        UpdatePipelineScheduleVariableRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a variable from a pipeline schedule. GitLab answers 202 and echoes the deleted variable;
    ///     the transport does not surface a body on <c>DELETE</c>, so nothing is returned.
    /// </summary>
    Task DeleteVariableAsync(ProjectId projectId, long pipelineScheduleId, string key,
        CancellationToken cancellationToken = default);
}