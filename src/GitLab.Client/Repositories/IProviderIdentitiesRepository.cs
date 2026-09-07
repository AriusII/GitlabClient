using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Provider identities resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProviderIdentitiesService), typeof(IProviderIdentitiesClient))]
internal interface IProviderIdentitiesRepository
{
    IAsyncEnumerable<GitLabProviderIdentity> ListSamlAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> GetSamlAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> UpdateSamlAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    Task DeleteSamlAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProviderIdentity> ListScimAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> GetScimAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    Task<GitLabProviderIdentity> UpdateScimAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    Task DeleteScimAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);
}