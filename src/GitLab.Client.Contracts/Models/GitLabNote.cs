using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A note (comment) on an issue, merge request or commit, as returned by the GitLab Notes and
///     Discussions APIs. Every member below <see cref="Body" /> is nullable because GitLab populates
///     them only for the noteable types and states where they apply.
///     <para>
///         GitLab's <c>commands_changes</c> member is an opaque object in the OpenAPI document and is retained
///         as JSON. <see cref="Position" /> uses the identical, documented diff-position shape shared with
///         draft notes and discussion creation.
///     </para>
/// </summary>
public sealed record GitLabNote
{
    public required long Id { get; init; }

    public required string Body { get; init; }

    /// <summary>GitLab's note class - <c>DiffNote</c>, <c>DiscussionNote</c>, or absent for a plain comment.</summary>
    public string? Type { get; init; }

    public GitLabUser? Author { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary><see langword="true" /> for a note GitLab generated itself (a label change, a milestone change, ...).</summary>
    public bool? System { get; init; }

    /// <summary>Whether this note participates in a resolvable discussion thread.</summary>
    public bool? Resolvable { get; init; }

    public bool? Resolved { get; init; }

    public GitLabUser? ResolvedBy { get; init; }

    public DateTimeOffset? ResolvedAt { get; init; }

    /// <summary>Database ID of the issue, merge request or snippet this note hangs off.</summary>
    public long? NoteableId { get; init; }

    /// <summary>Project-scoped IID of the noteable - the number that appears in GitLab's UI and in routes.</summary>
    public long? NoteableIid { get; init; }

    /// <summary><c>Issue</c>, <c>MergeRequest</c>, <c>Commit</c> or <c>Snippet</c>.</summary>
    public string? NoteableType { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>SHA of the commit a commit-discussion note is attached to.</summary>
    public string? CommitId { get; init; }

    /// <summary>
    ///     The diff location for a code-review note. It is absent for an ordinary note and can be
    ///     <see langword="null" /> when GitLab can no longer resolve the diff it was created against.
    /// </summary>
    public GitLabNotePosition? Position { get; init; }

    /// <summary>
    ///     Code suggestions embedded in this note. GitLab emits an array here for review notes, despite
    ///     the OpenAPI component currently naming the singular suggestion schema.
    /// </summary>
    public IReadOnlyList<GitLabSuggestion>? Suggestions { get; init; }

    public bool? Confidential { get; init; }

    /// <summary><see langword="true" /> for an internal note, visible only to project members.</summary>
    public bool? Internal { get; init; }

    /// <summary><see langword="true" /> for a note brought in by a project or group import.</summary>
    public bool? Imported { get; init; }

    /// <summary>Which importer produced an imported note - <c>github</c>, <c>bitbucket</c>, <c>gitea</c>, ...</summary>
    public string? ImportedFrom { get; init; }

    /// <summary>
    ///     The untyped side effects of GitLab quick actions contained in this note. GitLab does not define a
    ///     stable member schema for this object, so the original JSON is retained without lossy projection.
    /// </summary>
    public JsonElement? CommandsChanges { get; init; }
}