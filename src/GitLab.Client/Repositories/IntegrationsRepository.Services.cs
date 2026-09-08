using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Implements <see cref="IIntegrationsRepository.ListServicesAsync" /> - see that declaration's
///     remarks for why this exists alongside <see cref="ListAsync" />. Reuses the <c>Services</c> path
///     constant already declared on this partial class by <c>IntegrationsRepository.E.cs</c>.
/// </summary>
internal sealed partial class IntegrationsRepository
{
    public IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray,
            cancellationToken);
    }
}