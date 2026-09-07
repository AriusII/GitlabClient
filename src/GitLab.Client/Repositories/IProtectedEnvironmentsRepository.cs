using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Protected environments resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProtectedEnvironmentsService), typeof(IProtectedEnvironmentsClient))]
internal interface IProtectedEnvironmentsRepository
{
    IAsyncEnumerable<GitLabProtectedEnvironment> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> GetForProjectAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> ProtectForProjectAsync(ProjectId projectId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> UpdateForProjectAsync(ProjectId projectId, string name,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    Task UnprotectForProjectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProtectedEnvironment> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> GetForGroupAsync(GroupId groupId, string deploymentTier,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> ProtectForGroupAsync(GroupId groupId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedEnvironment> UpdateForGroupAsync(GroupId groupId, string deploymentTier,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default);

    Task UnprotectForGroupAsync(GroupId groupId, string deploymentTier, CancellationToken cancellationToken = default);
}