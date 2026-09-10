using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Endpoints;

internal sealed class PackagesRpmClient(IGitLabApiConnection connection) : IPackagesRpmClient
{
    public Task UploadAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rpm").Build(),
            file,
            null,
            cancellationToken);
    }

    public Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rpm")
                .Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRepositoryMetadataAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rpm")
                .Literal("repodata").Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageFileId,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rpm")
                .Segment(packageFileId).Escaped(fileName).Build(),
            cancellationToken);
    }
}