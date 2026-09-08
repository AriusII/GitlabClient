using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Dependency proxy" API area
///     (<c>/groups/:id/dependency_proxy/cache</c>) - the cache of upstream images and packages a group
///     proxies.
/// </summary>
public interface IDependencyProxyClient
{
    /// <summary>
    ///     Schedules every cached manifest and blob in a group's dependency proxy for deletion. GitLab
    ///     answers <c>202 Accepted</c> and does the work in the background, so the returned task completing
    ///     means the purge was queued, not that the cache is already empty. Requires the Owner role on the
    ///     group.
    /// </summary>
    Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a Maven package file through the project-level dependency proxy cache
    ///     (<c>GET /projects/:id/dependency_proxy/packages/maven/:path/:file_name</c>), streamed rather
    ///     than parsed. GitLab fetches and caches the file from the configured upstream Maven repository on
    ///     first request and serves the cached copy afterward.
    /// </summary>
    /// <param name="projectId">The project whose dependency proxy should serve the file.</param>
    /// <param name="path">
    ///     The Maven group/artifact/version path, e.g. <c>"com/example/mylib/1.0"</c>. May contain
    ///     multiple slash-separated segments; it is encoded as a single route segment.
    /// </param>
    /// <param name="fileName">The Maven package file name, e.g. <c>"mylib-1.0.pom"</c>.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The file body plus its media type. The caller owns it and must dispose it -
    ///     <c>await using</c> - once the body has been read.
    /// </returns>
    Task<GitLabFileResponse> DownloadMavenPackageFileAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads an npm package tarball through the project-level dependency proxy cache
    ///     (<c>GET /projects/:id/dependency_proxy/packages/npm/:package_name/-/:file_name</c>), streamed
    ///     rather than parsed. GitLab fetches and caches the tarball from the configured upstream npm
    ///     registry on first request and serves the cached copy afterward.
    /// </summary>
    /// <param name="projectId">The project whose dependency proxy should serve the tarball.</param>
    /// <param name="packageName">
    ///     The npm package name, including its scope when scoped (e.g. <c>"@scope/name"</c>).
    /// </param>
    /// <param name="fileName">The tarball file name, e.g. <c>"name-1.0.0.tgz"</c>.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The tarball body plus its media type. The caller owns it and must dispose it -
    ///     <c>await using</c> - once the body has been read.
    /// </returns>
    Task<GitLabFileResponse> DownloadNpmPackageTarballAsync(ProjectId projectId, string packageName, string fileName,
        CancellationToken cancellationToken = default);
}