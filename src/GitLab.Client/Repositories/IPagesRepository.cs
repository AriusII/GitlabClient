using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the GitLab Pages resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPagesService), typeof(IPagesClient))]
internal interface IPagesRepository
{
    Task<GitLabPagesSettings> GetSettingsAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabPagesSettings> UpdateSettingsAsync(ProjectId projectId, UpdatePagesSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task UnpublishAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task CheckAccessAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPagesDomainSummary> ListAllDomainsAsync(string? domain = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPagesDomain> ListDomainsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabPagesDomain> GetDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default);

    Task<GitLabPagesDomain> CreateDomainAsync(ProjectId projectId, CreatePagesDomainRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabPagesDomain> UpdateDomainAsync(ProjectId projectId, string domain, UpdatePagesDomainRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteDomainAsync(ProjectId projectId, string domain, CancellationToken cancellationToken = default);

    Task<GitLabPagesDomain> VerifyDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default);
}