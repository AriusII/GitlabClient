using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The list-all-active-integrations half of the <c>/projects/:id/services</c> alias, alongside the
///     slug-generic <c>GetServiceAsync</c> / <c>DisableServiceAsync</c> pair on
///     <see cref="IIntegrationsClient" /> part E. Byte-identical to <see cref="IIntegrationsClient.ListAsync" />,
///     which reaches the same integrations through the current <c>/integrations</c> path; the spec still
///     lists this as its own operation, so it is wrapped for completeness. Prefer
///     <see cref="IIntegrationsClient.ListAsync" /> unless a caller specifically needs to exercise the
///     legacy route.
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Streams the integrations active on a project, through the <c>/services</c> alias route.</summary>
    IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);
}