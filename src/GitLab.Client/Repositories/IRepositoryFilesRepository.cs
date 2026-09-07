using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the RepositoryFiles resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IRepositoryFilesService), typeof(IRepositoryFilesClient))]
internal interface IRepositoryFilesRepository
{
    Task<GitLabRepositoryFile> GetAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);

    Task<GitLabRepositoryFile> CreateAsync(ProjectId projectId, string filePath, CreateRepositoryFileRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRepositoryFile> UpdateAsync(ProjectId projectId, string filePath, UpdateRepositoryFileRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string filePath, string branch, string commitMessage,
        CancellationToken cancellationToken = default);

    Task<GitLabHeadResponse> GetMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawAsync(ProjectId projectId, string filePath, string? refName = null,
        bool? lfs = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBlameRange> GetBlameAsync(ProjectId projectId, string filePath, string refName,
        int? rangeStart = null, int? rangeEnd = null, CancellationToken cancellationToken = default);

    Task<GitLabHeadResponse> GetBlameMetadataAsync(ProjectId projectId, string filePath, string refName,
        CancellationToken cancellationToken = default);
}