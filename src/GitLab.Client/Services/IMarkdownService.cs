using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Markdown, sitting between the public <c>IMarkdownClient</c>
///     controller and <c>IMarkdownRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IMarkdownService
{
    Task<GitLabRenderedMarkdown> RenderAsync(RenderMarkdownRequest request,
        CancellationToken cancellationToken = default);
}