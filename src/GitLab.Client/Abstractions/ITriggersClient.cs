using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI triggers" API area: pipeline trigger token management
///     (<c>/projects/:id/triggers</c>) and firing a pipeline with one
///     (<c>/projects/:id/trigger/pipeline</c>).
/// </summary>
public interface ITriggersClient
{
    /// <summary>Lists the pipeline trigger tokens configured on the project (<c>GET /projects/:id/triggers</c>).</summary>
    IAsyncEnumerable<GitLabTrigger> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Gets a single pipeline trigger token by id (<c>GET /projects/:id/triggers/:trigger_id</c>).</summary>
    Task<GitLabTrigger> GetAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new pipeline trigger token (<c>POST /projects/:id/triggers</c>).</summary>
    Task<GitLabTrigger> CreateAsync(ProjectId projectId, CreateTriggerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a pipeline trigger token's description (<c>PUT /projects/:id/triggers/:trigger_id</c>). The
    ///     token value itself is immutable - see <see cref="UpdateTriggerRequest" />.
    /// </summary>
    Task<GitLabTrigger> UpdateAsync(ProjectId projectId, long triggerId, UpdateTriggerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a pipeline trigger token (<c>DELETE /projects/:id/triggers/:trigger_id</c>).</summary>
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