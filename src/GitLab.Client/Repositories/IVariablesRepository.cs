using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the CI/CD Variables resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IVariablesService), typeof(IVariablesClient))]
internal interface IVariablesRepository
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