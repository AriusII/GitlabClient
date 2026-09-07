using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ProtectedTags, sitting between the public
///     <c>IProtectedTagsClient</c> controller and <c>IProtectedTagsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IProtectedTagsService
{
    IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> ProtectAsync(ProjectId projectId, ProtectTagRequest request,
        CancellationToken cancellationToken = default);

    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);
}