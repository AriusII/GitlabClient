using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Applications resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IApplicationsService), typeof(IApplicationsClient))]
internal interface IApplicationsRepository
{
    IAsyncEnumerable<GitLabApplication> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> RenewSecretAsync(long id, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabApplication> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateForCurrentUserAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApplication> GetForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplication> UpdateForCurrentUserAsync(long id, UpdateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForCurrentUserAsync(long id, CancellationToken cancellationToken = default);
}