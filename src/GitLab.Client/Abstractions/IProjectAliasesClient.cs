using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Project alias" API area (<c>/project_aliases</c>) - alternative names a project
///     answers to over Git, so a repository imported from elsewhere keeps working under its old name.
///     <para>
///         Administrators only, on GitLab Premium and Ultimate. Anyone else gets
///         <see cref="Exceptions.GitLabForbiddenException" />, and an instance without the feature answers
///         <see cref="Exceptions.GitLabNotFoundException" />.
///     </para>
///     <para>
///         Note the asymmetry: an alias is created and read back with a numeric
///         <see cref="Models.GitLabProjectAlias.Id" />, but every route addresses it by
///         <see cref="Models.GitLabProjectAlias.Name" />.
///     </para>
/// </summary>
public interface IProjectAliasesClient
{
    /// <summary>Streams every project alias on the instance. Follows pagination automatically.</summary>
    IAsyncEnumerable<GitLabProjectAlias> ListAsync(ProjectAliasListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one alias by name. The name is percent-encoded by the route builder, so pass it raw even when
    ///     it looks like a project path.
    /// </summary>
    Task<GitLabProjectAlias> GetAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates an alias pointing at an existing project.</summary>
    Task<GitLabProjectAlias> CreateAsync(CreateProjectAliasRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an alias by name. The project it pointed at is untouched.</summary>
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);
}