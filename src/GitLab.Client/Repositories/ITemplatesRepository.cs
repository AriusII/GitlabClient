using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the instance Templates resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ITemplatesService), typeof(ITemplatesClient))]
internal interface ITemplatesRepository
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