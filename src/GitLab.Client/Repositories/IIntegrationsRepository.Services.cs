using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     The list-all-active-integrations half of the <c>/projects/:id/services</c> alias. The vendored
///     spec still lists <c>getApiV4ProjectsIdServices</c> as its own operation alongside
///     <c>getApiV4ProjectsIdIntegrations</c> (see <see cref="IntegrationsRepository.ListAsync" />), the
///     same way it lists a separate <c>getApiV4ProjectsIdServicesSlug</c> /
///     <c>deleteApiV4ProjectsIdServicesSlug</c> pair next to their <c>/integrations/:slug</c> equivalents
///     - see <c>IIntegrationsRepository.E.cs</c>'s <c>GetServiceAsync</c> / <c>DisableServiceAsync</c>,
///     which this follows for consistency. Byte-identical response shape to <see cref="ListAsync" />;
///     wrapped only because the spec still counts it as a distinct operation.
/// </summary>
internal partial interface IIntegrationsRepository
{
    IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);
}