using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class TriggersRepository(IGitLabApiConnection connection) : ITriggersRepository
{
    public IAsyncEnumerable<GitLabTrigger> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("triggers").Build(),
            GitLabJsonContext.Default.GitLabTriggerArray,
            cancellationToken);
    }

    public Task<GitLabTrigger> GetAsync(ProjectId projectId, long triggerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("triggers").Segment(triggerId).Build(),
            GitLabJsonContext.Default.GitLabTrigger,
            cancellationToken);
    }

    public Task<GitLabTrigger> CreateAsync(ProjectId projectId, CreateTriggerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("triggers").Build(),
            request,
            GitLabJsonContext.Default.CreateTriggerRequest,
            GitLabJsonContext.Default.GitLabTrigger,
            cancellationToken);
    }

    public Task<GitLabTrigger> UpdateAsync(ProjectId projectId, long triggerId, UpdateTriggerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("triggers").Segment(triggerId).Build(),
            request,
            GitLabJsonContext.Default.UpdateTriggerRequest,
            GitLabJsonContext.Default.GitLabTrigger,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("triggers").Segment(triggerId).Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Note the singular "trigger" segment: token management lives under <c>/triggers</c>, but firing a
    ///     pipeline lives under <c>/trigger/pipeline</c>.
    /// </summary>
    public Task<GitLabPipeline> TriggerPipelineAsync(ProjectId projectId, TriggerPipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("trigger")
                .Literal("pipeline")
                .Build(),
            request,
            GitLabJsonContext.Default.TriggerPipelineRequest,
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public Task<GitLabPipeline> TriggerPipelineForRefAsync(ProjectId projectId, string refName,
        TriggerPipelineRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("ref")
                .Escaped(refName)
                .Literal("trigger")
                .Literal("pipeline")
                .Build(),
            request,
            GitLabJsonContext.Default.TriggerPipelineRequest,
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }
}