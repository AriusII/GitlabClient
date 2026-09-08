using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Mirrors <c>IIntegrationsRepository.ListServicesAsync</c> (see that declaration for rationale);
///     its implementation is generated.
/// </summary>
internal partial interface IIntegrationsService
{
    IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);
}