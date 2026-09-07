using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for SAML group links, sitting between the public
///     <c>ISamlGroupLinksClient</c> controller and <c>ISamlGroupLinksRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated);
///     this is the seam where request validation, caching, or cross-resource composition would go once
///     the resource needs more than pass-through.
/// </summary>
internal interface ISamlGroupLinksService
{
    IAsyncEnumerable<GitLabSamlGroupLink> ListAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabSamlGroupLink> GetAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);

    Task<GitLabSamlGroupLink> CreateAsync(GroupId groupId, CreateSamlGroupLinkRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(GroupId groupId, string samlGroupName, string? provider = null,
        CancellationToken cancellationToken = default);
}