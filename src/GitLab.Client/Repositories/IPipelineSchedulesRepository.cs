using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the PipelineSchedules resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPipelineSchedulesService), typeof(IPipelineSchedulesClient))]
internal interface IPipelineSchedulesRepository
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