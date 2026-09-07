namespace GitLab.Client.Models;

/// <summary>
///     One related file or excerpt sent alongside the current file, to give
///     <c>POST /code_suggestions/completions</c> more of the surrounding codebase than the open buffer.
/// </summary>
public sealed record CodeSuggestionContextPart
{
    /// <summary>Whether <see cref="Content" /> is a whole file or an excerpt.</summary>
    public required CodeSuggestionContextType Type { get; init; }

    /// <summary>The file name or symbol this part came from. At most 255 characters.</summary>
    public required string Name { get; init; }

    /// <summary>The text itself. At most 600,000 characters.</summary>
    public required string Content { get; init; }
}