using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CI Triggers resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ITriggersService), typeof(ITriggersClient))]
internal interface ITriggersRepository
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