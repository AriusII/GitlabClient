using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CommitStatuses resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICommitStatusesService), typeof(ICommitStatusesClient))]
internal interface ICommitStatusesRepository
{
    IAsyncEnumerable<GitLabCommitStatus> ListAsync(ProjectId projectId, string sha,
        CommitStatusListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabCommitStatus> CreateAsync(ProjectId projectId, string sha, CreateCommitStatusRequest request,
        CancellationToken cancellationToken = default);
}