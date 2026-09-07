using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Releases, sitting between the public <c>IReleasesClient</c>
///     controller and <c>IReleasesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IReleasesService
{
    IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRelease> ListForGroupAsync(GroupId groupId, GroupReleaseListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabRelease> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabRelease> CreateAsync(ProjectId projectId, CreateReleaseRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRelease> UpdateAsync(ProjectId projectId, string tagName, UpdateReleaseRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabRelease> GenerateEvidenceAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabReleaseLink> ListLinksAsync(ProjectId projectId, string tagName,
        ReleaseLinkListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> GetLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> CreateLinkAsync(ProjectId projectId, string tagName, CreateReleaseLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> UpdateLinkAsync(ProjectId projectId, string tagName, long linkId,
        UpdateReleaseLinkRequest request, CancellationToken cancellationToken = default);

    Task DeleteLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);
}