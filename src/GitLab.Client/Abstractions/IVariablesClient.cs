using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI variables" API area for all three variable scopes: project
///     (<c>/projects/:id/variables</c>), group (<c>/groups/:id/variables</c>) and instance
///     (<c>/admin/ci/variables</c>).
///     <para>
///         The instance methods require an administrator token; every other token gets a <c>403</c>, so
///         a library consumer that is not running as an admin should treat them as unavailable rather
///         than as a transient failure.
///     </para>
///     <para>
///         Variable values are secrets. Nothing in this library logs a request body, and a variable
///         created with <see cref="CreateVariableRequest.MaskedAndHidden" /> comes back with a null
///         <see cref="GitLabVariable.Value" /> forever after - GitLab will not disclose it again, not even
///         to the token that created it.
///     </para>
/// </summary>
public interface IVariablesClient
{
    /// <summary>Streams every CI/CD variable defined on a project.</summary>
    IAsyncEnumerable<GitLabVariable> ListProjectVariablesAsync(ProjectId projectId,
        VariableListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one project variable. Supply <paramref name="environmentScope" /> to disambiguate a key that
    ///     exists in several environment scopes; without it GitLab returns whichever it matches first.
    /// </summary>
    Task<GitLabVariable> GetProjectVariableAsync(ProjectId projectId, string key, string? environmentScope = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project CI/CD variable.</summary>
    Task<GitLabVariable> CreateProjectVariableAsync(ProjectId projectId, CreateVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a project CI/CD variable.</summary>
    Task<GitLabVariable> UpdateProjectVariableAsync(ProjectId projectId, string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates one project CI/CD variable selected by key and its current environment scope. The filter
    ///     selects the existing variable; <see cref="UpdateVariableRequest.EnvironmentScope" /> in
    ///     <paramref name="request" /> is the scope to assign after the update and can therefore differ.
    /// </summary>
    Task<GitLabVariable> UpdateProjectVariableAsync(ProjectId projectId, string key, UpdateVariableRequest request,
        string? filterEnvironmentScope, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one project variable. Supply <paramref name="environmentScope" /> to target a specific
    ///     scope of a key that exists more than once.
    /// </summary>
    Task DeleteProjectVariableAsync(ProjectId projectId, string key, string? environmentScope = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every CI/CD variable defined on a group.</summary>
    IAsyncEnumerable<GitLabVariable> ListGroupVariablesAsync(GroupId groupId, VariableListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one group CI/CD variable when its key is unique across environment scopes. When it is not,
    ///     use the overload that selects the variable with an environment-scope filter.
    /// </summary>
    Task<GitLabVariable> GetGroupVariableAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one group CI/CD variable selected by key and environment scope. GitLab permits the same key
    ///     in several scopes, so <paramref name="filterEnvironmentScope" /> disambiguates that case through
    ///     <c>filter[environment_scope]</c>.
    /// </summary>
    Task<GitLabVariable> GetGroupVariableAsync(GroupId groupId, string key, string? filterEnvironmentScope,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a group CI/CD variable.</summary>
    Task<GitLabVariable> CreateGroupVariableAsync(GroupId groupId, CreateVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a group CI/CD variable when its key is unique across environment scopes. When it is not,
    ///     use the overload that selects the existing variable with an environment-scope filter.
    /// </summary>
    Task<GitLabVariable> UpdateGroupVariableAsync(GroupId groupId, string key, UpdateVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates one group CI/CD variable selected by key and its current environment scope. The filter
    ///     selects the existing variable; <see cref="UpdateVariableRequest.EnvironmentScope" /> in
    ///     <paramref name="request" /> is the scope to assign after the update and can therefore differ.
    /// </summary>
    Task<GitLabVariable> UpdateGroupVariableAsync(GroupId groupId, string key, UpdateVariableRequest request,
        string? filterEnvironmentScope, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one group CI/CD variable when its key is unique across environment scopes. When it is not,
    ///     use the overload that selects the variable with an environment-scope filter.
    /// </summary>
    Task DeleteGroupVariableAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one group CI/CD variable selected by key and environment scope, using
    ///     <c>filter[environment_scope]</c> to avoid deleting a same-named variable in another scope.
    /// </summary>
    Task DeleteGroupVariableAsync(GroupId groupId, string key, string? filterEnvironmentScope,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every instance-level CI/CD variable. Administrator only, and there is exactly one
    ///     instance scope, so this takes no id.
    /// </summary>
    IAsyncEnumerable<GitLabVariable> ListInstanceVariablesAsync(VariableListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one instance variable. Unlike the project form there is no environment-scope filter:
    ///     instance variables are always scoped to <c>*</c>, so a key identifies exactly one variable.
    /// </summary>
    Task<GitLabVariable> GetInstanceVariableAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Creates an instance variable with the fields accepted by GitLab's administrative route.</summary>
    Task<GitLabVariable> CreateInstanceVariableAsync(CreateInstanceVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an instance variable with the fields accepted by GitLab's administrative route.</summary>
    Task<GitLabVariable> UpdateInstanceVariableAsync(string key, UpdateInstanceVariableRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an instance variable.</summary>
    Task DeleteInstanceVariableAsync(string key, CancellationToken cancellationToken = default);
}