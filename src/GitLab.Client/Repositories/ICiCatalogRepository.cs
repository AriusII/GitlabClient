using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CI Catalog resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICiCatalogService), typeof(ICiCatalogClient))]
internal interface ICiCatalogRepository
{
    Task<GitLabCiCatalogPublishResult> PublishAsync(ProjectId projectId, CiCatalogPublishRequest request,
        CancellationToken cancellationToken = default);
}