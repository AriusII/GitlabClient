using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class RolloutsRepository(IGitLabApiConnection connection) : IRolloutsRepository
{
    public Task<GitLabCdRollout> IngestEventAsync(long id, IngestRolloutEventRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("rollouts")
                .Segment(id)
                .Build(),
            request,
            GitLabJsonContext.Default.IngestRolloutEventRequest,
            GitLabJsonContext.Default.GitLabCdRollout,
            cancellationToken);
    }
}