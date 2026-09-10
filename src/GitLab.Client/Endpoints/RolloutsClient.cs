using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class RolloutsClient(IGitLabApiConnection connection) : IRolloutsClient
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