namespace GitLab.Client.Models;

/// <summary>
///     A note (comment) on an issue, merge request or commit, as returned by the GitLab Notes and
///     Discussions APIs. Every member below <see cref="Body" /> is nullable because GitLab populates
///     them only for the noteable types and states where they apply.
///     <para>
///         GitLab's <c>position</c> and <c>commands_changes</c> members are deliberately absent: the
///         OpenAPI document declares both as a bare <c>type: object</c> with no properties, so no
///         source-generated DTO can be derived from them.
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

    public bool? Confidential { get; init; }

    /// <summary><see langword="true" /> for an internal note, visible only to project members.</summary>
    public bool? Internal { get; init; }

    /// <summary><see langword="true" /> for a note brought in by a project or group import.</summary>
    public bool? Imported { get; init; }

    /// <summary>Which importer produced an imported note - <c>github</c>, <c>bitbucket</c>, <c>gitea</c>, ...</summary>
    public string? ImportedFrom { get; init; }
}