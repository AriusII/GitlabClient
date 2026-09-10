namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /markdown</c>.</summary>
public sealed record RenderMarkdownRequest
{
    /// <summary>The Markdown source to render. The only member GitLab requires.</summary>
    public required string Text { get; init; }

    /// <summary>
    ///     Render with GitLab Flavored Markdown rather than plain CommonMark. Defaults to <c>false</c>
    ///     server-side, which means task lists, tables and GitLab references are not expanded unless this
    ///     is set.
    /// </summary>
    public bool? Gfm { get; init; }

    /// <summary>
    ///     The project whose context resolves GitLab Flavored Markdown references - the full path
    ///     (<c>group/subgroup/project</c>) or the numeric id as text. This is what turns <c>#42</c>,
    ///     <c>!7</c>, <c>@user</c> or <c>~bug</c> into links: without it those stay literal text, because
    ///     GitLab has nothing to resolve them against. Only meaningful together with
    ///     <see cref="Gfm" />, and only for projects the caller can read.
    /// </summary>
    public string? Project { get; init; }
}