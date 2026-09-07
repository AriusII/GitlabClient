namespace GitLab.Client.Models;

/// <summary>
///     The buffer the user is looking at while asking a <c>POST /chat/completions</c> question. Every
///     member is optional, unlike the code-completion equivalent
///     (<see cref="CodeSuggestionCurrentFile" />), and it carries the user's selection as well.
/// </summary>
public sealed record DuoChatCurrentFile
{
    /// <summary>The file's name. At most 1,000 characters.</summary>
    public string? FileName { get; init; }

    /// <summary>Everything before the cursor. At most 400,000 characters.</summary>
    public string? ContentAboveCursor { get; init; }

    /// <summary>Everything after the cursor. At most 400,000 characters.</summary>
    public string? ContentBelowCursor { get; init; }

    /// <summary>The text the user has selected, which is usually what the question is about. At most 400,000 characters.</summary>
    public string? SelectedText { get; init; }
}