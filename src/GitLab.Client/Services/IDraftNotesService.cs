using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Draft notes, sitting between the public <c>IDraftNotesClient</c>
///     controller and <c>IDraftNotesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IDraftNotesService
{
    IAsyncEnumerable<GitLabDraftNote> ListAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabDraftNote> GetAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    Task<GitLabDraftNote> CreateAsync(ProjectId projectId, long mergeRequestIid, CreateDraftNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabDraftNote> UpdateAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        UpdateDraftNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    Task PublishAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    Task PublishAllAsync(ProjectId projectId, long mergeRequestIid, PublishDraftNotesRequest request,
        CancellationToken cancellationToken = default);
}