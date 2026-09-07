using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the project uploads resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectUploadsService), typeof(IProjectUploadsClient))]
internal interface IProjectUploadsRepository
{
    IAsyncEnumerable<GitLabProjectUpload> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectUploadLink> UploadAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long uploadId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long uploadId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);

    Task DeleteBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);
}