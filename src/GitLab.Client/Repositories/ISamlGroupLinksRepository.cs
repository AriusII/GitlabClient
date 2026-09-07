using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the SAML group links resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISamlGroupLinksService), typeof(ISamlGroupLinksClient))]
internal interface ISamlGroupLinksRepository
{
    IAsyncEnumerable<GitLabSamlGroupLink> ListAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabSamlGroupLink> GetAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);

    Task<GitLabSamlGroupLink> CreateAsync(GroupId groupId, CreateSamlGroupLinkRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);
}