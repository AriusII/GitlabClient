using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Notes resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         GitLab hangs one identical note surface (list / get / create / update / delete) off seven
///         different parents - project issues, merge requests, snippets, vulnerabilities and wiki page
///         metas, plus group epics and group wiki page metas - so this interface is seven families of
///         the same five methods, distinguished only by the parent they name.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(INotesService), typeof(INotesClient))]
internal interface INotesRepository
{
    IAsyncEnumerable<GitLabNote> ListIssueNotesAsync(ProjectId projectId, long issueIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateIssueNoteAsync(ProjectId projectId, long issueIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListMergeRequestNotesAsync(ProjectId projectId, long mergeRequestIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListSnippetNotesAsync(ProjectId projectId, long snippetId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateSnippetNoteAsync(ProjectId projectId, long snippetId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListVulnerabilityNotesAsync(ProjectId projectId, long vulnerabilityId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListProjectWikiPageNotesAsync(ProjectId projectId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId,
        CreateNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListEpicNotesAsync(GroupId groupId, long epicIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateEpicNoteAsync(GroupId groupId, long epicIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateEpicNoteAsync(GroupId groupId, long epicIid, long noteId, UpdateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListGroupWikiPageNotesAsync(GroupId groupId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> CreateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);
}