using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's RubyGems registry (<c>/projects/:id/packages/rubygems/...</c>) - the endpoints
///     <c>gem</c> and <c>bundler</c> speak directly: the spec index, gemspecs, gem downloads, gem upload
///     and dependency resolution.
/// </summary>
public interface IPackagesRubyGemsClient
{
    /// <summary>
    ///     Downloads one of the registry's Marshal-format spec index files - the full index, the
    ///     latest-only index, or the prerelease-only index.
    /// </summary>
    Task<GitLabFileResponse> GetSpecIndexAsync(ProjectId projectId, GitLabRubyGemsSpecIndexFile file,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a single gem's gemspec, in Marshal format.</summary>
    Task<GitLabFileResponse> GetGemspecAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a <c>.gem</c> file.</summary>
    Task<GitLabFileResponse> DownloadGemAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The workhorse pre-upload authorization check for <see cref="UploadGemAsync" />. Direct callers
    ///     of this client do not need it - it exists for workhorse-fronted deployments, not as a
    ///     precondition <see cref="UploadGemAsync" /> itself requires.
    /// </summary>
    Task AuthorizeGemUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a <c>.gem</c> file (the shape <c>gem push</c> speaks). GitLab answers
    ///     <c>201 Created</c> with no body, so this method completes once the upload is accepted rather
    ///     than returning a package DTO.
    /// </summary>
    Task UploadGemAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves dependency metadata for the gems named in <paramref name="options" /> (or every gem in
    ///     the registry, if omitted). The response is a Marshalled array, not JSON.
    /// </summary>
    Task<GitLabFileResponse> GetDependenciesAsync(ProjectId projectId,
        RubyGemsDependencyListOptions? options = null, CancellationToken cancellationToken = default);
}