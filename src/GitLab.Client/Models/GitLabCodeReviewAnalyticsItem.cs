using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One open merge request's code-review metrics, as returned by <c>GET /analytics/code_review</c>.
/// </summary>
public sealed record GitLabCodeReviewAnalyticsItem
{
    public required long Id { get; init; }

    public required long Iid { get; init; }

    public required long ProjectId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string State { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     The milestone attached to the merge request, if any. GitLab's OpenAPI document does not spell
    ///     out this nested shape, so it is kept as a raw <see cref="JsonElement" /> rather than a guessed
    ///     record.
    /// </summary>
    public JsonElement? Milestone { get; init; }

    /// <summary>The merge request's author. See <see cref="Milestone" /> for why this stays untyped.</summary>
    public JsonElement? Author { get; init; }

    /// <summary>Users who approved the merge request. See <see cref="Milestone" /> for why this stays untyped.</summary>
    public JsonElement? ApprovedBy { get; init; }

    public int? NotesCount { get; init; }

    /// <summary>Time from the merge request's creation to its first review comment, in seconds.</summary>
    public int? ReviewTime { get; init; }

    /// <summary>
    ///     A rendered diff-size summary. GitLab's OpenAPI document types this as a plain string rather than
    ///     the structured <c>{ additions, deletions, ... }</c> object other endpoints use for diff stats.
    /// </summary>
    public string? DiffStats { get; init; }
}