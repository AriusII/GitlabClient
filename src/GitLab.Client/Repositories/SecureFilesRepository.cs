using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class SecureFilesRepository(IGitLabApiConnection connection) : ISecureFilesRepository
{
    public IAsyncEnumerable<GitLabSecureFile> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("secure_files")
                .Build(),
            GitLabJsonContext.Default.GitLabSecureFileArray,
            cancellationToken);
    }

    public Task<GitLabSecureFile> CreateAsync(ProjectId projectId, string name, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> formFields = new(StringComparer.Ordinal) { ["name"] = name };

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("secure_files").Build(),
            file,
            formFields,
            GitLabJsonContext.Default.GitLabSecureFile,
            cancellationToken);
    }

    public Task<GitLabSecureFile> GetAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("secure_files")
                .Segment(secureFileId)
                .Build(),
            GitLabJsonContext.Default.GitLabSecureFile,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("secure_files")
                .Segment(secureFileId)
                .Literal("download")
                .Build(),
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long secureFileId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("secure_files")
                .Segment(secureFileId)
                .Build(),
            cancellationToken);
    }
}