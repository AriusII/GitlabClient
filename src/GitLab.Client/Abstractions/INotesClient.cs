using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Notes" API area - the comments and system events that hang off a noteable.
///     <para>
///         GitLab exposes the same five operations (list, get, create, update, delete) against seven
///         different parents, and this interface names each parent explicitly rather than asking
///         callers to pass a noteable type string:
///         <c>/projects/:id/issues/:issue_iid/notes</c>,
///         <c>/projects/:id/merge_requests/:merge_request_iid/notes</c>,
///         <c>/projects/:id/snippets/:snippet_id/notes</c>,
///         <c>/projects/:id/vulnerabilities/:vulnerability_id/notes</c>,
///         <c>/projects/:id/wiki_pages/:wiki_page_meta_id/notes</c>,
///         <c>/groups/:id/epics/:epic_iid/notes</c> and
///         <c>/groups/:id/wiki_pages/:wiki_page_meta_id/notes</c>.
///     </para>
///     <para>
///         Both wiki-page families are keyed by the numeric <em>wiki page meta</em> ID, not by the page
///         slug - the slug-addressed wiki API is <see cref="IWikisClient" />.
///     </para>
///     <para>
///         A listing returns human comments <em>and</em> the system notes GitLab writes itself (label
///         changes, milestone changes, ...); filter them apart with
///         <see cref="NoteListOptions.ActivityFilter" /> or by checking <see cref="GitLabNote.System" />.
///     </para>
/// </summary>
public interface INotesClient
{
    /// <summary>Streams every note on an issue, newest first unless <paramref name="options" /> says otherwise.</summary>
    IAsyncEnumerable<GitLabNote> ListIssueNotesAsync(ProjectId projectId, long issueIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one issue note by its numeric ID.</summary>
    Task<GitLabNote> GetIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on an issue.</summary>
    Task<GitLabNote> CreateIssueNoteAsync(ProjectId projectId, long issueIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing issue note; only its author or a maintainer may.</summary>
    Task<GitLabNote> UpdateIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes an issue note.</summary>
    Task DeleteIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every note on a merge request, including the system notes for pushes and approvals.</summary>
    IAsyncEnumerable<GitLabNote> ListMergeRequestNotesAsync(ProjectId projectId, long mergeRequestIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one merge request note by its numeric ID.</summary>
    Task<GitLabNote> GetMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Comments on a merge request. Set <see cref="CreateNoteRequest.MergeRequestDiffHeadSha" /> to have
    ///     GitLab reject the comment if the merge request has moved on since it was written.
    /// </summary>
    Task<GitLabNote> CreateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing merge request note.</summary>
    Task<GitLabNote> UpdateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a merge request note.</summary>
    Task DeleteMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every note on a project snippet.</summary>
    IAsyncEnumerable<GitLabNote> ListSnippetNotesAsync(ProjectId projectId, long snippetId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one snippet note by its numeric ID.</summary>
    Task<GitLabNote> GetSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on a project snippet.</summary>
    Task<GitLabNote> CreateSnippetNoteAsync(ProjectId projectId, long snippetId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing snippet note.</summary>
    Task<GitLabNote> UpdateSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a snippet note.</summary>
    Task DeleteSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every note on a vulnerability. Vulnerabilities are an Ultimate feature.</summary>
    IAsyncEnumerable<GitLabNote> ListVulnerabilityNotesAsync(ProjectId projectId, long vulnerabilityId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one vulnerability note by its numeric ID.</summary>
    Task<GitLabNote> GetVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on a vulnerability.</summary>
    Task<GitLabNote> CreateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing vulnerability note.</summary>
    Task<GitLabNote> UpdateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a vulnerability note.</summary>
    Task DeleteVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every note on a project wiki page. <paramref name="wikiPageMetaId" /> is the numeric wiki
    ///     page meta ID, not the page slug.
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListProjectWikiPageNotesAsync(ProjectId projectId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one project wiki page note by its numeric ID.</summary>
    Task<GitLabNote> GetProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on a project wiki page.</summary>
    Task<GitLabNote> CreateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId,
        CreateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing project wiki page note.</summary>
    Task<GitLabNote> UpdateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a project wiki page note.</summary>
    Task DeleteProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every note on a group epic. Epics are a Premium and Ultimate feature.</summary>
    IAsyncEnumerable<GitLabNote> ListEpicNotesAsync(GroupId groupId, long epicIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one epic note by its numeric ID.</summary>
    Task<GitLabNote> GetEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on a group epic.</summary>
    Task<GitLabNote> CreateEpicNoteAsync(GroupId groupId, long epicIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing epic note.</summary>
    Task<GitLabNote> UpdateEpicNoteAsync(GroupId groupId, long epicIid, long noteId, UpdateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an epic note.</summary>
    Task DeleteEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every note on a group wiki page. <paramref name="wikiPageMetaId" /> is the numeric wiki
    ///     page meta ID, not the page slug.
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListGroupWikiPageNotesAsync(GroupId groupId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one group wiki page note by its numeric ID.</summary>
    Task<GitLabNote> GetGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Comments on a group wiki page.</summary>
    Task<GitLabNote> CreateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the body of an existing group wiki page note.</summary>
    Task<GitLabNote> UpdateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a group wiki page note.</summary>
    Task DeleteGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default);
}