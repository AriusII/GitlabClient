using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Member roles resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMemberRolesService), typeof(IMemberRolesClient))]
internal interface IMemberRolesRepository
{
    IAsyncEnumerable<GitLabMemberRole> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabMemberRole> CreateAsync(CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long memberRoleId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMemberRole> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabMemberRole> CreateForGroupAsync(GroupId groupId, CreateMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long memberRoleId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMemberRole> ListAdminRolesAsync(CancellationToken cancellationToken = default);

    Task<GitLabMemberRole> CreateAdminRoleAsync(CreateAdminMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAdminRoleAsync(long memberRoleId, CancellationToken cancellationToken = default);
}