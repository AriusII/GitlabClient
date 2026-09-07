using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Cargo package registry
///     (<c>/projects/:id/packages/cargo/...</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesCargoService), typeof(IPackagesCargoClient))]
internal interface IPackagesCargoRepository
{
    Task<GitLabFileResponse> GetSparseIndexForOneCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSparseIndexForTwoCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSparseIndexForThreeCharacterNameAsync(ProjectId projectId, string firstChar,
        string packageName, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSparseIndexAsync(ProjectId projectId, string prefix1, string prefix2,
        string packageName, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetConfigAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadCrateAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default);
}