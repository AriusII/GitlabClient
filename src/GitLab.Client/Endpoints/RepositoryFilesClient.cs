using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class RepositoryFilesClient(IGitLabApiConnection connection) : IRepositoryFilesClient
{
    // The pinned GitLab 19.4 OpenAPI schemas for these mutations require the binary part to be named "file".
    // Do not trust the caller-configurable GitLabFileUpload.FieldName here: GitLab otherwise accepts a request
    // under another name but cannot apply the file operation it describes.
    private const string FileFieldName = "file";

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
        return connection.PostMultipartFormAsync(
            FileRoute(projectId, filePath).Build(),
            request,
            GitLabJsonContext.Default.CreateRepositoryFileRequest,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task<GitLabRepositoryFile> CreateAsync(ProjectId projectId, string filePath, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            FileRoute(projectId, filePath).Build(),
            file with { FieldName = FileFieldName },
            null,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task<GitLabRepositoryFile> UpdateAsync(ProjectId projectId, string filePath,
        UpdateRepositoryFileRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutMultipartFormAsync(
            FileRoute(projectId, filePath).Build(),
            request,
            GitLabJsonContext.Default.UpdateRepositoryFileRequest,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task<GitLabRepositoryFile> UpdateAsync(ProjectId projectId, string filePath, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            FileRoute(projectId, filePath).Build(),
            file with { FieldName = FileFieldName },
            null,
            GitLabJsonContext.Default.GitLabRepositoryFile,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string filePath, string branch, string commitMessage,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(projectId, filePath,
            new DeleteRepositoryFileRequest { Branch = branch, CommitMessage = commitMessage }, cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string filePath, DeleteRepositoryFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.DeleteAsync(
            FileRoute(projectId, filePath)
                .Query("branch", request.Branch)
                .Query("commit_message", request.CommitMessage)
                .Query("start_branch", request.StartBranch)
                .Query("author_email", request.AuthorEmail)
                .Query("author_name", request.AuthorName)
                .Query("last_commit_id", request.LastCommitId)
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
        if (rangeStart.HasValue != rangeEnd.HasValue)
        {
            throw new ArgumentException("GitLab requires range[start] and range[end] together.", nameof(rangeEnd));
        }

        if (rangeStart is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeStart), "The first blamed line must be positive.");
        }

        if (rangeEnd is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeEnd), "The last blamed line must be positive.");
        }

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