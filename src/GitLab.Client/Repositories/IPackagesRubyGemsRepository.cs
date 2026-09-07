using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the RubyGems package registry
///     (<c>/projects/:id/packages/rubygems/...</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesRubyGemsService), typeof(IPackagesRubyGemsClient))]
internal interface IPackagesRubyGemsRepository
{
    Task<GitLabFileResponse> GetSpecIndexAsync(ProjectId projectId, GitLabRubyGemsSpecIndexFile file,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetGemspecAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGemAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    Task AuthorizeGemUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task UploadGemAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetDependenciesAsync(ProjectId projectId,
        RubyGemsDependencyListOptions? options = null, CancellationToken cancellationToken = default);
}