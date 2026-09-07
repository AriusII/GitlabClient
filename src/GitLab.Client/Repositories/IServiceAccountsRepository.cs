using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Service accounts resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IServiceAccountsService), typeof(IServiceAccountsClient))]
internal interface IServiceAccountsRepository
{
    IAsyncEnumerable<GitLabServiceAccount> ListAsync(ServiceAccountListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateAsync(CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateAsync(long userId, UpdateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabServiceAccount> ListForGroupAsync(GroupId groupId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> GetForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateForGroupAsync(GroupId groupId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateForGroupAsync(GroupId groupId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabServiceAccount> ListForProjectAsync(ProjectId projectId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> GetForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> CreateForProjectAsync(ProjectId projectId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabServiceAccount> UpdateForProjectAsync(ProjectId projectId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default);
}