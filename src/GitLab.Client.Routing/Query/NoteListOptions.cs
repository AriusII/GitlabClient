using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters shared by every note listing - <c>GET /projects/:id/:noteable/:noteable_id/notes</c> and
///     <c>GET /groups/:id/:noteable/:noteable_id/notes</c>. GitLab declares exactly the same three
///     parameters on all seven noteable types, so one options record covers them all.
/// </summary>
[GitLabQuery]
public readonly record struct NoteListOptions
{
    /// <summary>Either "created_at" or "updated_at". GitLab defaults to "created_at".</summary>
    public string? OrderBy { get; init; }

    /// <summary>Either "asc" or "desc". GitLab defaults to "desc".</summary>
    public string? Sort { get; init; }

    /// <summary>
    ///     Which kinds of note come back: "all_notes" (the default), "only_comments" for human comments
    ///     only, or "only_activity" for the system notes GitLab writes itself (label changes, milestone
    ///     changes, and so on).
    /// </summary>
    public string? ActivityFilter { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Which page of results to return (1-based).</summary>
    public int? Page { get; init; }
}