namespace GitLab.Client.Models;

/// <summary>
///     The result of <c>POST /markdown</c>: the caller's Markdown, rendered to HTML by the same pipeline
///     GitLab itself uses for issue and merge request descriptions.
/// </summary>
public sealed record GitLabRenderedMarkdown
{
    /// <summary>
    ///     The rendered HTML, for example <c>&lt;p dir="auto"&gt;Hello world!&lt;/p&gt;</c>. Nullable
    ///     because the spec declares no member as required; an empty <c>text</c> renders to an empty
    ///     fragment rather than to null in practice.
    /// </summary>
    public string? Html { get; init; }
}