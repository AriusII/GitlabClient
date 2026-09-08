using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

internal sealed class DependencyProxyRepository(IGitLabApiConnection connection) : IDependencyProxyRepository
{
    public Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("dependency_proxy").Literal("cache")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadMavenPackageFileAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default)
    {
        // "path" is the Maven group/artifact/version path (e.g. "com/example/mylib/1.0"), captured by
        // GitLab as a single wildcard route segment. Following the same convention as the project-level
        // Maven package registry route (PackagesGenericRepository.ProjectMavenRoute), it is escaped as one
        // unit: Uri.EscapeDataString percent-encodes the embedded '/' characters, which folds the whole
        // multi-segment path back into one logical path segment rather than several - exactly what a
        // literal, unescaped '/' would otherwise be misread as by the route builder.
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("dependency_proxy")
                .Literal("packages").Literal("maven").Escaped(path).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadNpmPackageTarballAsync(ProjectId projectId, string packageName,
        string fileName, CancellationToken cancellationToken = default)
    {
        // packageName is escaped as one segment for the same reason PackagesNpmRepository escapes it:
        // npm's scoped-package convention folds "@scope/name" into one logical identifier by
        // percent-encoding the '/' it contains, which is exactly what an npm client does before sending
        // the request.
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("dependency_proxy")
                .Literal("packages").Literal("npm").Escaped(packageName).Literal("-").Escaped(fileName).Build(),
            cancellationToken);
    }
}