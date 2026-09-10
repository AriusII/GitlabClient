using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class CiCatalogClient(IGitLabApiConnection connection) : ICiCatalogClient
{
    public Task<GitLabCiCatalogPublishResult> PublishAsync(ProjectId projectId, CiCatalogPublishRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("catalog")
                .Literal("publish")
                .Build(),
            request,
            GitLabJsonContext.Default.CiCatalogPublishRequest,
            GitLabJsonContext.Default.GitLabCiCatalogPublishResult,
            cancellationToken);
    }
}