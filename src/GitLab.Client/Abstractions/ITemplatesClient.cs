using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab instance "Templates" API area (<c>/templates/dockerfiles</c>,
///     <c>/templates/gitignores</c>, <c>/templates/gitlab_ci_ymls</c>, <c>/templates/licenses</c>) - the
///     file templates every project on the instance can start from.
///     <para>
///         All four kinds share one route shape - list, then retrieve one template - but not one payload
///         shape: the three plain kinds list as <see cref="GitLabTemplateSummary" /> (key and name only)
///         and retrieve as <see cref="GitLabTemplate" /> (name and content), while licenses both list and
///         retrieve as the fully described <see cref="GitLabLicenseTemplate" />.
///     </para>
///     <para>
///         These are the instance-wide templates only. Custom CI/CD templates and any template a project
///         or group defines for itself are not visible here - use the project templates API for those.
///         GitLab's own documentation notes that the project template endpoints supersede these, and that
///         the instance endpoints are scheduled for removal in API v5; nothing in this library depends on
///         them, so a future v5 does not strand a caller who has moved on.
///     </para>
///     <para>
///         A user with only the Guest role cannot read these endpoints, which answer <c>401</c> rather
///         than <c>403</c> when the token cannot see them.
///     </para>
/// </summary>
public interface ITemplatesClient
{
    /// <summary>Streams every Dockerfile template the instance ships, as key/name pairs.</summary>
    IAsyncEnumerable<GitLabTemplateSummary> ListDockerfilesAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one Dockerfile template with its body. Pass the template's <c>name</c> raw - names
    ///     legitimately contain <c>+</c> and <c>.</c> (<c>C++</c>, <c>Node.gitignore</c>) and the route
    ///     builder percent-encodes them.
    /// </summary>
    Task<GitLabTemplate> GetDockerfileAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Streams every <c>.gitignore</c> template the instance ships, as key/name pairs.</summary>
    IAsyncEnumerable<GitLabTemplateSummary> ListGitignoresAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one <c>.gitignore</c> template with its body, by name.</summary>
    Task<GitLabTemplate> GetGitignoreAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every GitLab CI/CD YAML template the instance ships, as key/name pairs. Custom CI/CD
    ///     templates defined by a project or group are deliberately not included.
    /// </summary>
    IAsyncEnumerable<GitLabTemplateSummary> ListCiYmlsAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one GitLab CI/CD YAML template with its body, by name.</summary>
    Task<GitLabTemplate> GetCiYmlAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the open source license templates, each already carrying its full metadata and text.
    ///     Set <see cref="LicenseTemplateListOptions.Popular" /> to get only the short, commonly offered
    ///     list.
    /// </summary>
    IAsyncEnumerable<GitLabLicenseTemplate> ListLicensesAsync(LicenseTemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one license template by its <see cref="GitLabLicenseTemplate.Key" /> (for example,
    ///     <c>apache-2.0</c> or <c>mit</c>), optionally with its copyright placeholders already filled in.
    /// </summary>
    /// <param name="name">
    ///     The license key returned by <see cref="ListLicensesAsync" />, not
    ///     <see cref="GitLabLicenseTemplate.Name" />. It is percent-encoded in the route.
    /// </param>
    /// <param name="project">
    ///     The copyrighted project's name. Substituted for the <c>[project]</c> placeholder in
    ///     <see cref="GitLabLicenseTemplate.Content" />; templates that have no such placeholder ignore it.
    /// </param>
    /// <param name="fullname">
    ///     The full name of the copyright holder. Substituted for the <c>[fullname]</c> placeholder in
    ///     <see cref="GitLabLicenseTemplate.Content" />. GitLab spells this parameter <c>fullname</c>, one
    ///     word.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabLicenseTemplate> GetLicenseAsync(string name, string? project = null, string? fullname = null,
        CancellationToken cancellationToken = default);
}