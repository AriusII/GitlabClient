using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class VariablesClient(IGitLabApiConnection connection) : IVariablesClient
{
    /// <summary>
    ///     GitLab filters a variable by environment scope through a nested query parameter whose name carries
    ///     literal square brackets. They are part of the parameter <em>name</em>, which
    ///     <see cref="GitLabRouteBuilder.Query(string, string?)" /> appends verbatim - only the value is escaped.
    /// </summary>
    private const string EnvironmentScopeFilter = "filter[environment_scope]";

    public IAsyncEnumerable<GitLabVariable> ListProjectVariablesAsync(ProjectId projectId,
        VariableListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("variables")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabVariableArray,
            cancellationToken);
    }

    public Task<GitLabVariable> GetProjectVariableAsync(ProjectId projectId, string key,
        string? environmentScope = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, environmentScope)
                .Build(),
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> CreateProjectVariableAsync(ProjectId projectId, CreateVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("variables")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> UpdateProjectVariableAsync(ProjectId projectId, string key,
        UpdateVariableRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateProjectVariableAsync(projectId, key, request, null, cancellationToken);
    }

    public Task<GitLabVariable> UpdateProjectVariableAsync(ProjectId projectId, string key,
        UpdateVariableRequest request, string? filterEnvironmentScope, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, filterEnvironmentScope)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    /// <summary>
    ///     GitLab answers this one with <c>200 OK</c> and the deleted variable as the body rather than the
    ///     usual <c>204</c>; <see cref="IGitLabApiConnection.DeleteAsync" /> discards it either way.
    /// </summary>
    public Task DeleteProjectVariableAsync(ProjectId projectId, string key, string? environmentScope = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, environmentScope)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVariable> ListGroupVariablesAsync(GroupId groupId,
        VariableListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("variables")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabVariableArray,
            cancellationToken);
    }

    public Task<GitLabVariable> GetGroupVariableAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default)
    {
        return GetGroupVariableAsync(groupId, key, null, cancellationToken);
    }

    public Task<GitLabVariable> GetGroupVariableAsync(GroupId groupId, string key, string? filterEnvironmentScope,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, filterEnvironmentScope)
                .Build(),
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> CreateGroupVariableAsync(GroupId groupId, CreateVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("variables")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> UpdateGroupVariableAsync(GroupId groupId, string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        return UpdateGroupVariableAsync(groupId, key, request, null,
            cancellationToken);
    }

    public Task<GitLabVariable> UpdateGroupVariableAsync(GroupId groupId, string key, UpdateVariableRequest request,
        string? filterEnvironmentScope, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, filterEnvironmentScope)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task DeleteGroupVariableAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default)
    {
        return DeleteGroupVariableAsync(groupId, key, null,
            cancellationToken);
    }

    public Task DeleteGroupVariableAsync(GroupId groupId, string key, string? filterEnvironmentScope,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("variables")
                .Escaped(key)
                .Query(EnvironmentScopeFilter, filterEnvironmentScope)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVariable> ListInstanceVariablesAsync(VariableListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            InstanceVariables()
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabVariableArray,
            cancellationToken);
    }

    public Task<GitLabVariable> GetInstanceVariableAsync(string key, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceVariables()
                .Escaped(key)
                .Build(),
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> CreateInstanceVariableAsync(CreateInstanceVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            InstanceVariables().Build(),
            request,
            GitLabJsonContext.Default.CreateInstanceVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    public Task<GitLabVariable> UpdateInstanceVariableAsync(string key, UpdateInstanceVariableRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            InstanceVariables()
                .Escaped(key)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateInstanceVariableRequest,
            GitLabJsonContext.Default.GitLabVariable,
            cancellationToken);
    }

    /// <summary>
    ///     Like its project sibling, GitLab answers with <c>200 OK</c> and the deleted variable as the body
    ///     rather than <c>204</c>; <see cref="IGitLabApiConnection.DeleteAsync" /> discards it either way -
    ///     which is just as well, since that body would carry the secret.
    /// </summary>
    public Task DeleteInstanceVariableAsync(string key, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            InstanceVariables()
                .Escaped(key)
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     The instance scope has no id segment - there is one CI/CD instance - so its five routes share
    ///     the same three fixed words. "admin" and "ci" come from the route template, never from a caller.
    /// </summary>
    private static GitLabRouteBuilder InstanceVariables()
    {
        return GitLabRouteBuilder.Create("admin")
            .Literal("ci")
            .Literal("variables");
    }
}