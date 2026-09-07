using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI/CD Variables, sitting between the public
///     <c>IVariablesClient</c> controller and <c>IVariablesRepository</c>'s raw GitLab access. Mirrors the
///     repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface IVariablesService
{
    IAsyncEnumerable<GitLabVariable> ListProjectVariablesAsync(ProjectId projectId,
        VariableListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabVariable> GetProjectVariableAsync(ProjectId projectId, string key, string? environmentScope = null,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> CreateProjectVariableAsync(ProjectId projectId, CreateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> UpdateProjectVariableAsync(ProjectId projectId, string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteProjectVariableAsync(ProjectId projectId, string key, string? environmentScope = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabVariable> ListGroupVariablesAsync(GroupId groupId, VariableListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> GetGroupVariableAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> CreateGroupVariableAsync(GroupId groupId, CreateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> UpdateGroupVariableAsync(GroupId groupId, string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteGroupVariableAsync(GroupId groupId, string key, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabVariable> ListInstanceVariablesAsync(VariableListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> GetInstanceVariableAsync(string key, CancellationToken cancellationToken = default);

    Task<GitLabVariable> CreateInstanceVariableAsync(CreateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabVariable> UpdateInstanceVariableAsync(string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteInstanceVariableAsync(string key, CancellationToken cancellationToken = default);
}