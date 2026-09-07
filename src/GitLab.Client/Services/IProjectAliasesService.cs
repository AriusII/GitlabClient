using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Project aliases, sitting between the public
///     <c>IProjectAliasesClient</c> controller and <c>IProjectAliasesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IProjectAliasesService
{
    IAsyncEnumerable<GitLabProjectAlias> ListAsync(ProjectAliasListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectAlias> GetAsync(string name, CancellationToken cancellationToken = default);

    Task<GitLabProjectAlias> CreateAsync(CreateProjectAliasRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string name, CancellationToken cancellationToken = default);
}