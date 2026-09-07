using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Tags, sitting between the public <c>ITagsClient</c>
///     controller and <c>ITagsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ITagsService
{
    IAsyncEnumerable<GitLabTag> ListAsync(ProjectId projectId, TagListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTag> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabTag> CreateAsync(ProjectId projectId, CreateTagRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabTagSignature> GetSignatureAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);
}