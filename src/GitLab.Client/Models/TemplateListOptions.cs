using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for the instance template listings that take no filter of their own
///     (<c>GET /templates/dockerfiles</c>, <c>/templates/gitignores</c>, <c>/templates/gitlab_ci_ymls</c>).
///     License listings filter on popularity as well - see <see cref="LicenseTemplateListOptions" />.
/// </summary>
[GitLabQuery]
public readonly record struct TemplateListOptions
{
    /// <summary>
    ///     Page size. Pagination itself is automatic - the listings stream every page - so this only
    ///     tunes how many templates each round trip carries.
    /// </summary>
    public int? PerPage { get; init; }
}