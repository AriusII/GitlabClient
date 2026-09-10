namespace GitLab.Client.Models;

/// <summary>
///     A single instance file template with its body, as returned by
///     <c>GET /templates/dockerfiles/:name</c>, <c>GET /templates/gitignores/:name</c> and
///     <c>GET /templates/gitlab_ci_ymls/:name</c>. License templates carry considerably more metadata
///     and have their own shape - see <see cref="GitLabLicenseTemplate" />.
/// </summary>
public sealed record GitLabTemplate
{
    /// <summary>The template's name, echoing the <c>{name}</c> path parameter - <c>Ruby</c>, <c>C++</c>.</summary>
    public required string Name { get; init; }

    /// <summary>The template body, verbatim - the Dockerfile, <c>.gitignore</c> or <c>.gitlab-ci.yml</c> text.</summary>
    public required string Content { get; init; }
}