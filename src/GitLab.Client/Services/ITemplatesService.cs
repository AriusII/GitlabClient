using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the instance Templates area, sitting between the public
///     <c>ITemplatesClient</c> controller and <c>ITemplatesRepository</c>'s raw GitLab access. Mirrors the
///     repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface ITemplatesService
{
    IAsyncEnumerable<GitLabTemplateSummary> ListDockerfilesAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTemplate> GetDockerfileAsync(string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabTemplateSummary> ListGitignoresAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTemplate> GetGitignoreAsync(string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabTemplateSummary> ListCiYmlsAsync(TemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTemplate> GetCiYmlAsync(string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabLicenseTemplate> ListLicensesAsync(LicenseTemplateListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabLicenseTemplate> GetLicenseAsync(string name, string? project = null, string? fullname = null,
        CancellationToken cancellationToken = default);
}