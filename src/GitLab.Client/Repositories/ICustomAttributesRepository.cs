using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Custom attributes resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICustomAttributesService), typeof(ICustomAttributesClient))]
internal interface ICustomAttributesRepository
{
    IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForUserAsync(long userId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForUserAsync(long userId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForUserAsync(long userId, string key, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCustomAttribute> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForGroupAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForGroupAsync(GroupId groupId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string key, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCustomAttribute> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> GetForProjectAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabCustomAttribute> SetForProjectAsync(ProjectId projectId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, string key, CancellationToken cancellationToken = default);
}