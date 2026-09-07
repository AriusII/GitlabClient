using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class RepositoryFilesRepository(IGitLabApiConnection connection) : IRepositoryFilesRepository
{
    public Task<GitLabRepositoryFile> GetAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            FileRoute(projectId, filePath)
                .Query("ref", refName)
                .Build(),
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task<GitLabRepositoryFile> CreateAsync(ProjectId projectId, string filePath,
        CreateRepositoryFileRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            FileRoute(projectId, filePath).Build(),
            request,
            GitLabJsonContext.Default.CreateRepositoryFileRequest,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task<GitLabRepositoryFile> UpdateAsync(ProjectId projectId, string filePath,
        UpdateRepositoryFileRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            FileRoute(projectId, filePath).Build(),
            request,
            GitLabJsonContext.Default.UpdateRepositoryFileRequest,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string filePath, string branch, string commitMessage,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            FileRoute(projectId, filePath)
                .Query("branch", branch)
                .Query("commit_message", commitMessage)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabHeadResponse> GetMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default)
    {
        return connection.HeadAsync(
            FileRoute(projectId, filePath)
                .Query("ref", refName)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawAsync(ProjectId projectId, string filePath, string? refName = null,
        bool? lfs = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            FileRoute(projectId, filePath)
                .Literal("raw")
                .Query("ref", refName)
                .Query("lfs", lfs)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBlameRange> GetBlameAsync(ProjectId projectId, string filePath, string refName,
        int? rangeStart = null, int? rangeEnd = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            BlameRoute(projectId, filePath)
                .Query("ref", refName)
                .Query("range[start]", rangeStart)
                .Query("range[end]", rangeEnd)
                .Build(),
            GitLabJsonContext.Default.GitLabBlameRangeArray,
            cancellationToken);
    }

    public Task<GitLabHeadResponse> GetBlameMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default)
    {
        return connection.HeadAsync(
            BlameRoute(projectId, filePath)
                .Query("ref", refName)
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     The path is percent-encoded here, never appended verbatim: a repository path is caller-supplied
    ///     free text full of slashes and dots (<c>src/App.cs</c>), and an unescaped one becomes a 404 on a
    ///     route that does not exist rather than an obvious client bug.
    /// </summary>
    private static GitLabRouteBuilder FileRoute(ProjectId projectId, string filePath)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("repository")
            .Literal("files")
            .Escaped(filePath);
    }

    private static GitLabRouteBuilder BlameRoute(ProjectId projectId, string filePath)
    {
        return FileRoute(projectId, filePath).Literal("blame");
    }
}