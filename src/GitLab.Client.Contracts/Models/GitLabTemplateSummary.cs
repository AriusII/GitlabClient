namespace GitLab.Client.Models;

/// <summary>
///     One entry of an instance template listing (<c>GET /templates/dockerfiles</c>,
///     <c>/templates/gitignores</c>, <c>/templates/gitlab_ci_ymls</c>) - the template's identity only.
///     <para>
///         The listing deliberately carries no <c>content</c>: fetch the body with the matching
///         <c>Get*Async</c> call, passing <see cref="Name" /> (not <see cref="Key" />) as the route
///         segment, because that is what the <c>{name}</c> path parameter matches.
///     </para>
/// </summary>
public sealed record GitLabTemplateSummary
{
    /// <summary>The template's stable identifier - <c>mit</c>, <c>Node</c>, <c>Ruby</c>.</summary>
    public required string Key { get; init; }

    /// <summary>
    ///     The template's display name, and the value the <c>{name}</c> path parameter of the retrieve
    ///     endpoints expects - <c>MIT License</c>, <c>Node</c>, <c>C++</c>.
    /// </summary>
    public required string Name { get; init; }
}