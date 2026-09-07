namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /glql</c>.</summary>
public sealed record ExecuteGlqlQueryRequest
{
    /// <summary>
    ///     The whole GLQL code block - the YAML front matter (<c>display</c>, <c>fields</c>, <c>limit</c>)
    ///     plus the <c>query</c> itself - exactly as it would appear inside a <c>```glql</c> fence in a
    ///     description or wiki page.
    /// </summary>
    public required string GlqlYaml { get; init; }

    /// <summary>
    ///     Forward-pagination cursor. Pass the <see cref="GitLabGlqlPageInfo.EndCursor" /> of the previous
    ///     response to fetch the next page; GLQL paginates by cursor rather than by GitLab's usual
    ///     <c>Link</c> header, so this resource cannot stream.
    /// </summary>
    public string? After { get; init; }
}