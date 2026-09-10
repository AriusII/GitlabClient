using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/wikis/:slug</c> and <c>PUT /groups/:id/wikis/:slug</c>. Every
///     member is optional and an unset one is omitted from the payload rather than sent as null, so a
///     partial update cannot silently blank a field it did not mean to touch.
/// </summary>
public sealed record UpdateWikiPageRequest
{
    public string? Title { get; init; }

    public string? Content { get; init; }

    /// <summary>Markup format. Omit to leave the page's current format unchanged.</summary>
    public GitLabWikiFormat? Format { get; init; }

    /// <summary>YAML front matter to store with the page; the spec types it as an untyped object.</summary>
    public JsonElement? FrontMatter { get; init; }
}