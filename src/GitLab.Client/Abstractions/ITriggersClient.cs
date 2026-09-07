using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI triggers" API area: pipeline trigger token management
///     (<c>/projects/:id/triggers</c>) and firing a pipeline with one
///     (<c>/projects/:id/trigger/pipeline</c>).
/// </summary>
public interface ITriggersClient
{
    IAsyncEnumerable<GitLabTrigger> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabTrigger> GetAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default);

    Task<GitLabTrigger> CreateAsync(ProjectId projectId, CreateTriggerRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabTrigger> UpdateAsync(ProjectId projectId, long triggerId, UpdateTriggerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a new pipeline on the project default branch. Authentication comes from
    ///     <see cref="TriggerPipelineRequest.Token" /> - a trigger token or a CI job token - rather than from
    ///     the client credential configured in <c>AddGitLabClient</c>.
    /// </summary>
    Task<GitLabPipeline> TriggerPipelineAsync(ProjectId projectId, TriggerPipelineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a new pipeline on a specific branch or tag. Authentication comes from
    ///     <see cref="TriggerPipelineRequest.Token" />, not from the configured client credential.
    /// </summary>
    Task<GitLabPipeline> TriggerPipelineForRefAsync(ProjectId projectId, string refName,
        TriggerPipelineRequest request, CancellationToken cancellationToken = default);
}