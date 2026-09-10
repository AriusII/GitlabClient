namespace GitLab.Client.Models;

/// <summary>
///     The kind of template a project exposes
///     (<c>GET /projects/:id/templates/:type</c>, <c>GET /projects/:id/templates/:type/:name</c>).
///     <para>
///         A closed vocabulary the spec enumerates on the path parameter, so it is modelled as an enum
///         rather than a free-text segment: an unknown type is a <c>404</c> from GitLab, and a compile
///         error is a cheaper way to find that out. It never appears in a JSON payload - the repository
///         projects it onto the route.
///     </para>
/// </summary>
public enum GitLabProjectTemplateType
{
    /// <summary>Dockerfile templates.</summary>
    Dockerfiles,

    /// <summary><c>.gitignore</c> templates.</summary>
    Gitignores,

    /// <summary><c>.gitlab-ci.yml</c> templates.</summary>
    GitlabCiYmls,

    /// <summary>Open-source licence templates, the only type whose placeholders can be expanded.</summary>
    Licenses,

    /// <summary>Issue description templates committed under <c>.gitlab/issue_templates</c>.</summary>
    Issues,

    /// <summary>Merge request description templates committed under <c>.gitlab/merge_request_templates</c>.</summary>
    MergeRequests
}