using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class TemplatesClient(IGitLabApiConnection connection) : ITemplatesClient
{
    private const string Dockerfiles = "dockerfiles";
    private const string Gitignores = "gitignores";
    private const string CiYmls = "gitlab_ci_ymls";
    private const string Licenses = "licenses";

    public IAsyncEnumerable<GitLabTemplateSummary> ListDockerfilesAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return ListTemplatesAsync(Dockerfiles, options, cancellationToken);
    }

    public Task<GitLabTemplate> GetDockerfileAsync(string name, CancellationToken cancellationToken = default)
    {
        return GetTemplateAsync(Dockerfiles, name, cancellationToken);
    }

    public IAsyncEnumerable<GitLabTemplateSummary> ListGitignoresAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return ListTemplatesAsync(Gitignores, options, cancellationToken);
    }

    public Task<GitLabTemplate> GetGitignoreAsync(string name, CancellationToken cancellationToken = default)
    {
        return GetTemplateAsync(Gitignores, name, cancellationToken);
    }

    public IAsyncEnumerable<GitLabTemplateSummary> ListCiYmlsAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return ListTemplatesAsync(CiYmls, options, cancellationToken);
    }

    public Task<GitLabTemplate> GetCiYmlAsync(string name, CancellationToken cancellationToken = default)
    {
        return GetTemplateAsync(CiYmls, name, cancellationToken);
    }

    public IAsyncEnumerable<GitLabLicenseTemplate> ListLicensesAsync(LicenseTemplateListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            Templates(Licenses).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabLicenseTemplateArray,
            cancellationToken);
    }

    public Task<GitLabLicenseTemplate> GetLicenseAsync(string name, string? project = null,
        string? fullname = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Templates(Licenses).Escaped(name).Query("project", project).Query("fullname", fullname).Build(),
            GitLabJsonContext.Default.GitLabLicenseTemplate,
            cancellationToken);
    }

    /// <summary>
    ///     The three plain template kinds - Dockerfiles, gitignores and CI/CD YAML - differ only in one
    ///     path word and share a response shape, so they share these two calls rather than repeating the
    ///     same body four times. Licenses do not: they carry a richer entity and a <c>popular</c> filter.
    /// </summary>
    private IAsyncEnumerable<GitLabTemplateSummary> ListTemplatesAsync(string kind, TemplateListOptions? options,
        CancellationToken cancellationToken)
    {
        return connection.GetPagedAsync(
            Templates(kind).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabTemplateSummaryArray,
            cancellationToken);
    }

    private Task<GitLabTemplate> GetTemplateAsync(string kind, string name, CancellationToken cancellationToken)
    {
        // Escaped, not Literal: template names are caller-supplied free text and legitimately contain
        // '+' and '.' ("C++", "Node.gitignore"). An unescaped '+' would reach GitLab as a space.
        return connection.GetAsync(
            Templates(kind).Escaped(name).Build(),
            GitLabJsonContext.Default.GitLabTemplate,
            cancellationToken);
    }

    private static GitLabRouteBuilder Templates(string kind)
    {
        return GitLabRouteBuilder.Create("templates").Literal(kind);
    }
}