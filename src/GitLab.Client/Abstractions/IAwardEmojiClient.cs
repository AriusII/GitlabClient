using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Award emoji" API area - emoji reactions on the four awardables (issues, merge
///     requests, project snippets and group epics) and on the notes of each
///     (<c>/projects/:id/issues/:iid/award_emoji</c> and friends).
///     <para>
///         Reactions are add/remove only: GitLab has no update operation for them. Only the user who
///         added a reaction (or an administrator) may delete it; anyone else gets a
///         <c>GitLabForbiddenException</c>.
///     </para>
/// </summary>
public interface IAwardEmojiClient
{
    /// <summary>Streams the reactions on an issue (<c>GET /projects/:id/issues/:issue_iid/award_emoji</c>).</summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on an issue by its award id.</summary>
    Task<GitLabAwardEmoji> GetForIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to an issue. The emoji name carries no colons.</summary>
    Task<GitLabAwardEmoji> AddToIssueAsync(ProjectId projectId, long issueIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from an issue.</summary>
    Task DeleteFromIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a comment of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/notes/:note_id/award_emoji</c>).
    /// </summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on an issue comment by its award id.</summary>
    Task<GitLabAwardEmoji> GetForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to an issue comment.</summary>
    Task<GitLabAwardEmoji> AddToIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from an issue comment.</summary>
    Task DeleteFromIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/award_emoji</c>).
    /// </summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on a merge request by its award id.</summary>
    Task<GitLabAwardEmoji> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to a merge request. The emoji name carries no colons.</summary>
    Task<GitLabAwardEmoji> AddToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from a merge request.</summary>
    Task DeleteFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a comment of a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/notes/:note_id/award_emoji</c>).
    /// </summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid,
        long noteId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on a merge request comment by its award id.</summary>
    Task<GitLabAwardEmoji> GetForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        long awardId, CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to a merge request comment.</summary>
    Task<GitLabAwardEmoji> AddToMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from a merge request comment.</summary>
    Task DeleteFromMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a project snippet
    ///     (<c>GET /projects/:id/snippets/:snippet_id/award_emoji</c>).
    /// </summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="snippetId">
    ///     The snippet's <b>id</b>. Snippets are the one awardable addressed by id rather than by iid.
    /// </param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on a project snippet by its award id.</summary>
    Task<GitLabAwardEmoji> GetForSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to a project snippet. The emoji name carries no colons.</summary>
    Task<GitLabAwardEmoji> AddToSnippetAsync(ProjectId projectId, long snippetId, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from a project snippet.</summary>
    Task DeleteFromSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a comment of a project snippet
    ///     (<c>GET /projects/:id/snippets/:snippet_id/notes/:note_id/award_emoji</c>).
    /// </summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on a snippet comment by its award id.</summary>
    Task<GitLabAwardEmoji> GetForSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to a snippet comment.</summary>
    Task<GitLabAwardEmoji> AddToSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from a snippet comment.</summary>
    Task DeleteFromSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on a group epic (<c>GET /groups/:id/epics/:epic_iid/award_emoji</c>).
    /// </summary>
    /// <param name="groupId">The group's numeric id or its namespaced path.</param>
    /// <param name="epicIid">The epic's group-scoped <b>iid</b>, not its global id.</param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    /// <remarks>
    ///     Epics are a Premium feature and the epics themselves are deprecated in favour of work items,
    ///     but the reaction routes are the only way to read an epic's reactions over REST today.
    /// </remarks>
    IAsyncEnumerable<GitLabAwardEmoji> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on an epic by its award id.</summary>
    Task<GitLabAwardEmoji> GetForEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to an epic.</summary>
    Task<GitLabAwardEmoji> AddToEpicAsync(GroupId groupId, long epicIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from an epic.</summary>
    Task DeleteFromEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the reactions on an epic comment
    ///     (<c>GET /groups/:id/epics/:epic_iid/notes/:note_id/award_emoji</c>).
    /// </summary>
    IAsyncEnumerable<GitLabAwardEmoji> ListForEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one reaction on an epic comment by its award id.</summary>
    Task<GitLabAwardEmoji> GetForEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reaction to an epic comment.</summary>
    Task<GitLabAwardEmoji> AddToEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's own reactions from an epic comment.</summary>
    Task DeleteFromEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);
}