using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PagesRepository(IGitLabApiConnection connection) : IPagesRepository
{
    public Task<GitLabPagesSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PagesRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabPagesSettings,
            cancellationToken);
    }

    public Task<GitLabPagesSettings> UpdateSettingsAsync(ProjectId projectId, UpdatePagesSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            PagesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.UpdatePagesSettingsRequest,
            GitLabJsonContext.Default.GitLabPagesSettings,
            cancellationToken);
    }

    public Task UnpublishAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(PagesRoute(projectId).Build(), cancellationToken);
    }

    /// <summary>
    ///     The one endpoint in the API whose answer is the status code alone: it returns <c>200</c> with an
    ///     empty body, so it goes through the body-less GET rather than the deserializing overload.
    /// </summary>
    public Task CheckAccessAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pages_access").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPagesDomainSummary> ListAllDomainsAsync(string? domain = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("pages").Literal("domains").Query("domain", domain).Build(),
            GitLabJsonContext.Default.GitLabPagesDomainSummaryArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPagesDomain> ListDomainsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            DomainsRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabPagesDomainArray,
            cancellationToken);
    }

    public Task<GitLabPagesDomain> GetDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            DomainsRoute(projectId).Escaped(domain).Build(),
            GitLabJsonContext.Default.GitLabPagesDomain,
            cancellationToken);
    }

    public Task<GitLabPagesDomain> CreateDomainAsync(ProjectId projectId, CreatePagesDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            DomainsRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreatePagesDomainRequest,
            GitLabJsonContext.Default.GitLabPagesDomain,
            cancellationToken);
    }

    public Task<GitLabPagesDomain> UpdateDomainAsync(ProjectId projectId, string domain,
        UpdatePagesDomainRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            DomainsRoute(projectId).Escaped(domain).Build(),
            request,
            GitLabJsonContext.Default.UpdatePagesDomainRequest,
            GitLabJsonContext.Default.GitLabPagesDomain,
            cancellationToken);
    }

    public Task DeleteDomainAsync(ProjectId projectId, string domain, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(DomainsRoute(projectId).Escaped(domain).Build(), cancellationToken);
    }

    public Task<GitLabPagesDomain> VerifyDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            DomainsRoute(projectId).Escaped(domain).Literal("verify").Build(),
            GitLabJsonContext.Default.GitLabPagesDomain,
            cancellationToken);
    }

    private static GitLabRouteBuilder PagesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pages");
    }

    /// <summary>
    ///     A custom domain is caller-supplied free text whose dots and (for a wildcard) <c>*</c> must reach
    ///     GitLab intact, so the domain is always <c>Escaped</c>, never <c>Literal</c>.
    /// </summary>
    private static GitLabRouteBuilder DomainsRoute(ProjectId projectId)
    {
        return PagesRoute(projectId).Literal("domains");
    }
}