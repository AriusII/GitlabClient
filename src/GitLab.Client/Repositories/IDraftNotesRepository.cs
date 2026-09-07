using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Draft notes resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Draft notes are per-author: every operation here acts on the pending notes of the authenticated
///         user only, so <see cref="ListAsync" /> is not "every pending review comment on this merge
///         request".
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IDraftNotesService), typeof(IDraftNotesClient))]
internal interface IDraftNotesRepository
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