using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectUploadsClient(IGitLabApiConnection connection) : IProjectUploadsClient
{
    public IAsyncEnumerable<GitLabProjectUpload> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Build(),
            GitLabJsonContext.Default.GitLabProjectUploadArray,
            cancellationToken);
    }

    public Task<GitLabProjectUploadLink> UploadAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Build(),
            file,
            null,
            GitLabJsonContext.Default.GitLabProjectUploadLink,
            cancellationToken);
    }

    public Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long uploadId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Segment(uploadId).Build(),
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long uploadId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Segment(uploadId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Escaped(secret)
                .Escaped(filename).Build(),
            cancellationToken);
    }

    public Task DeleteBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("uploads").Escaped(secret)
                .Escaped(filename).Build(),
            cancellationToken);
    }
}