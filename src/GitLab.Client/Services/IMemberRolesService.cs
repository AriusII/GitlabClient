using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Member roles, sitting between the public <c>IMemberRolesClient</c>
///     controller and <c>IMemberRolesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IMemberRolesService
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