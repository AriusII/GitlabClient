namespace GitLab.Client.Models;

/// <summary>
///     A code suggestion attached to a merge request review comment, as returned by the GitLab Suggestions
///     API (<c>/suggestions</c>). Suggestion IDs are instance-wide, not project-scoped.
/// </summary>
public sealed record GitLabSuggestion
{
    public required long Id { get; init; }

    /// <summary>First line of the diff the suggestion replaces.</summary>
    public int? FromLine { get; init; }

    /// <summary>Last line of the diff the suggestion replaces.</summary>
    public int? ToLine { get; init; }

    /// <summary>
    ///     Whether the suggestion can still be applied. GitLab spells this member <c>appliable</c>, with one
    ///     "p"; the name here matches the wire deliberately.
    /// </summary>
    public bool? Appliable { get; init; }

    public bool? Applied { get; init; }

    /// <summary>The original content the suggestion replaces.</summary>
    public string? FromContent { get; init; }

    /// <summary>The proposed replacement content.</summary>
    public string? ToContent { get; init; }
}