using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Wikis, sitting between the public <c>IWikisClient</c>
///     controller and <c>IWikisRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IWikisService
{
    IAsyncEnumerable<GitLabWikiPage> ListForProjectAsync(ProjectId projectId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> GetForProjectAsync(ProjectId projectId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> CreateForProjectAsync(ProjectId projectId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> UpdateForProjectAsync(ProjectId projectId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    Task<GitLabWikiAttachment> UploadAttachmentForProjectAsync(ProjectId projectId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabWikiPage> ListForGroupAsync(GroupId groupId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> GetForGroupAsync(GroupId groupId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> CreateForGroupAsync(GroupId groupId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> UpdateForGroupAsync(GroupId groupId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default);

    Task<GitLabWikiAttachment> UploadAttachmentForGroupAsync(GroupId groupId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);
}