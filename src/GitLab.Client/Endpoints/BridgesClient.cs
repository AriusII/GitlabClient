using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Direct transport for GitLab's legacy pipeline bridge endpoint. The supported replacement lives on
///     <see cref="IPipelinesClient" />; this class deliberately contains no duplicate response or query mapping.
/// </summary>
internal sealed class BridgesClient(IGitLabApiConnection connection) : IBridgesClient
{
    public IAsyncEnumerable<GitLabBridge> ListAsync(ProjectId projectId, long pipelineId,
        TriggerJobListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("pipelines")
                .Segment(pipelineId)
                .Literal("bridges")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabBridgeArray,
            cancellationToken);
    }
}