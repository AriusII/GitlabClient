using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Projects job token scope resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IJobTokenScopeService), typeof(IJobTokenScopeClient))]
internal interface IJobTokenScopeRepository
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