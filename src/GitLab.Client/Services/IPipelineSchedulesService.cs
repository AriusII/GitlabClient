using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for PipelineSchedules, sitting between the public
///     <c>IPipelineSchedulesClient</c> controller and <c>IPipelineSchedulesRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is
///     the seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPipelineSchedulesService
{
    IAsyncEnumerable<GitLabPipelineSchedule> ListAsync(ProjectId projectId,
        PipelineScheduleListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabPipelineSchedule> GetAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default);

    Task<GitLabPipelineSchedule> CreateAsync(ProjectId projectId, CreatePipelineScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPipelineSchedule> UpdateAsync(ProjectId projectId, long pipelineScheduleId,
        UpdatePipelineScheduleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long pipelineScheduleId, CancellationToken cancellationToken = default);

    Task PlayAsync(ProjectId projectId, long pipelineScheduleId, CancellationToken cancellationToken = default);

    Task<GitLabPipelineSchedule> TakeOwnershipAsync(ProjectId projectId, long pipelineScheduleId,
        CancellationToken cancellationToken = default);

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