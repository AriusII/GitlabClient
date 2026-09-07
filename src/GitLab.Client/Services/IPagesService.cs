using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for GitLab Pages, sitting between the public <c>IPagesClient</c>
///     controller and <c>IPagesRepository</c>'s raw GitLab access. Mirrors the repository's method shapes
///     1:1 today (its implementation is generated); this is the seam where request validation, caching, or
///     cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IPagesService
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