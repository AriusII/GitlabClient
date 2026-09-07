namespace GitLab.Client.Models;

/// <summary>
///     The file the cursor is in, as sent to <c>POST /code_suggestions/completions</c>. The model
///     completes at the seam between <see cref="ContentAboveCursor" /> and
///     <see cref="ContentBelowCursor" />.
/// </summary>
public sealed record CodeSuggestionCurrentFile
{
    /// <summary>
    ///     The file's name, which is how the language is inferred. At most 255 characters.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>Everything before the cursor. At most 400,000 characters.</summary>
    public required string ContentAboveCursor { get; init; }

    /// <summary>Everything after the cursor. At most 400,000 characters.</summary>
    public string? ContentBelowCursor { get; init; }
}