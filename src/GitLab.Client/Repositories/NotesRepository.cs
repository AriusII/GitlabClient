using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     GitLab hangs one identical note surface off seven different parents, and the parent only ever
///     changes the route prefix. So each public method does exactly two things - name its parent, and
///     name its verb - while the five verbs and the two prefix shapes are each written once below.
///     That is what keeps <c>notes</c> and the collection words spelled correctly in one place instead
///     of giving thirty-five call sites the chance to typo one into a 404.
/// </summary>
internal sealed class NotesRepository(IGitLabApiConnection connection) : INotesRepository
{
    private const string Epics = "epics";
    private const string Issues = "issues";
    private const string MergeRequests = "merge_requests";
    private const string Snippets = "snippets";
    private const string Vulnerabilities = "vulnerabilities";
    private const string WikiPages = "wiki_pages";

    public IAsyncEnumerable<GitLabNote> ListIssueNotesAsync(ProjectId projectId, long issueIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(ProjectNotes(projectId, Issues, issueIid), options, cancellationToken);
    }

    public Task<GitLabNote> GetIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(ProjectNotes(projectId, Issues, issueIid), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateIssueNoteAsync(ProjectId projectId, long issueIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(ProjectNotes(projectId, Issues, issueIid), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(ProjectNotes(projectId, Issues, issueIid), noteId, request, cancellationToken);
    }

    public Task DeleteIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(ProjectNotes(projectId, Issues, issueIid), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListMergeRequestNotesAsync(ProjectId projectId, long mergeRequestIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(ProjectNotes(projectId, MergeRequests, mergeRequestIid), options, cancellationToken);
    }

    public Task<GitLabNote> GetMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(ProjectNotes(projectId, MergeRequests, mergeRequestIid), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid,
        CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return CreateAsync(ProjectNotes(projectId, MergeRequests, mergeRequestIid), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(ProjectNotes(projectId, MergeRequests, mergeRequestIid), noteId, request,
            cancellationToken);
    }

    public Task DeleteMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(ProjectNotes(projectId, MergeRequests, mergeRequestIid), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListSnippetNotesAsync(ProjectId projectId, long snippetId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(ProjectNotes(projectId, Snippets, snippetId), options, cancellationToken);
    }

    public Task<GitLabNote> GetSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(ProjectNotes(projectId, Snippets, snippetId), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateSnippetNoteAsync(ProjectId projectId, long snippetId, CreateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(ProjectNotes(projectId, Snippets, snippetId), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(ProjectNotes(projectId, Snippets, snippetId), noteId, request, cancellationToken);
    }

    public Task DeleteSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(ProjectNotes(projectId, Snippets, snippetId), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListVulnerabilityNotesAsync(ProjectId projectId, long vulnerabilityId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(ProjectNotes(projectId, Vulnerabilities, vulnerabilityId), options, cancellationToken);
    }

    public Task<GitLabNote> GetVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(ProjectNotes(projectId, Vulnerabilities, vulnerabilityId), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId,
        CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return CreateAsync(ProjectNotes(projectId, Vulnerabilities, vulnerabilityId), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(ProjectNotes(projectId, Vulnerabilities, vulnerabilityId), noteId, request,
            cancellationToken);
    }

    public Task DeleteVulnerabilityNoteAsync(ProjectId projectId, long vulnerabilityId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(ProjectNotes(projectId, Vulnerabilities, vulnerabilityId), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListProjectWikiPageNotesAsync(ProjectId projectId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(ProjectNotes(projectId, WikiPages, wikiPageMetaId), options, cancellationToken);
    }

    public Task<GitLabNote> GetProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(ProjectNotes(projectId, WikiPages, wikiPageMetaId), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId,
        CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return CreateAsync(ProjectNotes(projectId, WikiPages, wikiPageMetaId), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(ProjectNotes(projectId, WikiPages, wikiPageMetaId), noteId, request, cancellationToken);
    }

    public Task DeleteProjectWikiPageNoteAsync(ProjectId projectId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(ProjectNotes(projectId, WikiPages, wikiPageMetaId), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListEpicNotesAsync(GroupId groupId, long epicIid,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(GroupNotes(groupId, Epics, epicIid), options, cancellationToken);
    }

    public Task<GitLabNote> GetEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(GroupNotes(groupId, Epics, epicIid), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateEpicNoteAsync(GroupId groupId, long epicIid, CreateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(GroupNotes(groupId, Epics, epicIid), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateEpicNoteAsync(GroupId groupId, long epicIid, long noteId, UpdateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(GroupNotes(groupId, Epics, epicIid), noteId, request, cancellationToken);
    }

    public Task DeleteEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(GroupNotes(groupId, Epics, epicIid), noteId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListGroupWikiPageNotesAsync(GroupId groupId, long wikiPageMetaId,
        NoteListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return ListAsync(GroupNotes(groupId, WikiPages, wikiPageMetaId), options, cancellationToken);
    }

    public Task<GitLabNote> GetGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(GroupNotes(groupId, WikiPages, wikiPageMetaId), noteId, cancellationToken);
    }

    public Task<GitLabNote> CreateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId,
        CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return CreateAsync(GroupNotes(groupId, WikiPages, wikiPageMetaId), request, cancellationToken);
    }

    public Task<GitLabNote> UpdateGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(GroupNotes(groupId, WikiPages, wikiPageMetaId), noteId, request, cancellationToken);
    }

    public Task DeleteGroupWikiPageNoteAsync(GroupId groupId, long wikiPageMetaId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync(GroupNotes(groupId, WikiPages, wikiPageMetaId), noteId, cancellationToken);
    }

    /// <summary>
    ///     <c>/projects/:id/:noteable/:noteable_id/notes</c>. <paramref name="noteableCollection" /> is one of
    ///     the private constants above - a fixed word from GitLab's route template, never caller text, which
    ///     is why it goes through <c>Literal</c> rather than <c>Escaped</c>.
    /// </summary>
    private static GitLabRouteBuilder ProjectNotes(ProjectId projectId, string noteableCollection, long noteableId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal(noteableCollection)
            .Segment(noteableId)
            .Literal("notes");
    }

    /// <summary><c>/groups/:id/:noteable/:noteable_id/notes</c> - the group-scoped half of the same shape.</summary>
    private static GitLabRouteBuilder GroupNotes(GroupId groupId, string noteableCollection, long noteableId)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal(noteableCollection)
            .Segment(noteableId)
            .Literal("notes");
    }

    private IAsyncEnumerable<GitLabNote> ListAsync(GitLabRouteBuilder notes, NoteListOptions? options,
        CancellationToken cancellationToken)
    {
        return connection.GetPagedAsync(
            notes.QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    private Task<GitLabNote> GetAsync(GitLabRouteBuilder notes, long noteId, CancellationToken cancellationToken)
    {
        return connection.GetAsync(
            notes.Segment(noteId).Build(),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    private Task<GitLabNote> CreateAsync(GitLabRouteBuilder notes, CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        return connection.PostAsync(
            notes.Build(),
            request,
            GitLabJsonContext.Default.CreateNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    private Task<GitLabNote> UpdateAsync(GitLabRouteBuilder notes, long noteId, UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        return connection.PutAsync(
            notes.Segment(noteId).Build(),
            request,
            GitLabJsonContext.Default.UpdateNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    /// <summary>
    ///     GitLab answers a note deletion with <c>200</c> and the deleted note as the body rather than the
    ///     usual <c>204</c>. The body is discarded: a caller already holds everything it said, and returning
    ///     a tombstone would make this the one delete in the library with a return value.
    /// </summary>
    private Task DeleteAsync(GitLabRouteBuilder notes, long noteId, CancellationToken cancellationToken)
    {
        return connection.DeleteAsync(notes.Segment(noteId).Build(), cancellationToken);
    }
}