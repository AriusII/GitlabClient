using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Cargo package registry, sitting between the public
///     <c>IPackagesCargoClient</c> controller and <c>IPackagesCargoRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesCargoService
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