using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class CiCatalogRepository(IGitLabApiConnection connection) : ICiCatalogRepository
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