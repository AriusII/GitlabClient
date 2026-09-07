using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Markdown" API area (<c>POST /markdown</c>) - renders Markdown to HTML through the
///     same pipeline GitLab uses for issue and merge request descriptions, so a client can preview text
///     exactly as GitLab will show it rather than reimplementing GitLab Flavored Markdown.
///     <para>
///         The endpoint requires authentication like every other, and GitLab gates its availability behind
///         a feature flag: the spec's own tag description calls it "available for testing, but not ready
///         for production use". A <see cref="Exceptions.GitLabNotFoundException" /> here can therefore mean
///         "the flag is off on this instance" rather than "wrong route".
///     </para>
/// </summary>
public interface IMarkdownClient
{
    /// <summary>
    ///     Renders Markdown to HTML (<c>POST /markdown</c>).
    /// </summary>
    /// <remarks>
    ///     Two members of the request work together and are easy to get wrong alone.
    ///     <see cref="RenderMarkdownRequest.Gfm" /> defaults to <see langword="false" /> server-side, which
    ///     renders plain CommonMark; and <see cref="RenderMarkdownRequest.Project" /> is what gives GitLab
    ///     Flavored Markdown a context to resolve references against, so that <c>#42</c>, <c>!7</c>,
    ///     <c>@user</c> and <c>~bug</c> become links to that project's issue, merge request, user and
    ///     label instead of staying literal text. Rendering with a project the caller cannot read is a
    ///     <see cref="Exceptions.GitLabNotFoundException" />.
    ///     <para>
    ///         The returned HTML is GitLab's own sanitized output, but it is still HTML built from text a
    ///         caller supplied - treat it as untrusted if the Markdown did not come from you.
    ///     </para>
    /// </remarks>
    Task<GitLabRenderedMarkdown> RenderAsync(RenderMarkdownRequest request,
        CancellationToken cancellationToken = default);
}