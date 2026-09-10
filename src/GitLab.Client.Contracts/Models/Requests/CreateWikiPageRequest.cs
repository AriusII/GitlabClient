using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/wikis</c> and <c>POST /groups/:id/wikis</c>. GitLab derives
///     the new page's slug from <see cref="Title" />, so read it back off the returned
///     <see cref="GitLabWikiPage" /> rather than predicting it.
/// </summary>
public sealed record CreateWikiPageRequest
{
    public required string Title { get; init; }

    public required string Content { get; init; }

    /// <summary>Markup format. Omit to use GitLab's Markdown default.</summary>
    public GitLabWikiFormat? Format { get; init; }

    /// <summary>YAML front matter to store with the page; the spec types it as an untyped object.</summary>
    public JsonElement? FrontMatter { get; init; }
}