using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A wiki page, as returned by the GitLab Wikis API for a project (<c>/projects/:id/wikis</c>) or a
///     group (<c>/groups/:id/wikis</c>).
///     <para>
///         One type covers both wire shapes the spec declares: the list endpoint returns
///         <c>APIEntitiesWikiPageBasic</c> — slug, title, format and meta id only — while get/create/update
///         return the full <c>APIEntitiesWikiPage</c>. <see cref="Content" />, <see cref="Encoding" /> and
///         <see cref="FrontMatter" /> are therefore null after a list unless it was made with
///         <c>withContent: true</c>.
///     </para>
/// </summary>
public sealed record GitLabWikiPage
{
    /// <summary>
    ///     The page's address within the wiki. GitLab derives it from the title on create (spaces become
    ///     hyphens), so it usually differs from <see cref="Title" />, and a nested page's slug contains
    ///     '/' — for example <c>home/setup</c>.
    /// </summary>
    public required string Slug { get; init; }

    public required string Title { get; init; }

    /// <summary>Markup format of <see cref="Content" />: "markdown", "rdoc", "asciidoc" or "org".</summary>
    public string? Format { get; init; }

    public long? WikiPageMetaId { get; init; }

    /// <summary>The page body. Null on a list response unless the call asked for content.</summary>
    public string? Content { get; init; }

    public string? Encoding { get; init; }

    /// <summary>
    ///     The page's YAML front matter. The spec types it as an untyped object, so it is captured as a raw
    ///     <see cref="JsonElement" /> rather than being forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? FrontMatter { get; init; }
}