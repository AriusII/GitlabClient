using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the RubyGems package registry, sitting between the public
///     <c>IPackagesRubyGemsClient</c> controller and <c>IPackagesRubyGemsRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IPackagesRubyGemsService
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