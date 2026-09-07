using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI Triggers, sitting between the public <c>ITriggersClient</c>
///     controller and <c>ITriggersRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ITriggersService
{
    IAsyncEnumerable<GitLabTrigger> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabTrigger> GetAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default);

    Task<GitLabTrigger> CreateAsync(ProjectId projectId, CreateTriggerRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabTrigger> UpdateAsync(ProjectId projectId, long triggerId, UpdateTriggerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long triggerId, CancellationToken cancellationToken = default);

    Task<GitLabPipeline> TriggerPipelineAsync(ProjectId projectId, TriggerPipelineRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPipeline> TriggerPipelineForRefAsync(ProjectId projectId, string refName,
        TriggerPipelineRequest request, CancellationToken cancellationToken = default);
}