using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Projects job token scope resource, sitting between the public
///     <c>IJobTokenScopeClient</c> controller and <c>IJobTokenScopeRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IJobTokenScopeService
{
    Task<GitLabProjectJobTokenScope> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task UpdateAsync(ProjectId projectId, UpdateProjectJobTokenScopeRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProject> AddToAllowlistAsync(ProjectId projectId, AddProjectToJobTokenAllowlistRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveFromAllowlistAsync(ProjectId projectId, long targetProjectId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabJobTokenScopeGroup> ListGroupsAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabJobTokenScopeGroup> AddGroupToAllowlistAsync(ProjectId projectId,
        AddGroupToJobTokenAllowlistRequest request, CancellationToken cancellationToken = default);

    Task RemoveGroupFromAllowlistAsync(ProjectId projectId, long targetGroupId,
        CancellationToken cancellationToken = default);
}