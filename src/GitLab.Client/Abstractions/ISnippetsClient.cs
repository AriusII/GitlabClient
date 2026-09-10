using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Snippets" API area - both halves of it: personal snippets under
///     <c>/snippets</c>, and project snippets under <c>/projects/:id/snippets</c>. The two surfaces
///     return the same entity and take the same request bodies, so the methods differ only by the
///     <c>ForProject</c> suffix and the extra <see cref="ProjectId" />.
///     <para>
///         A snippet is a git repository, not a text field: it can hold several files, and
///         <see cref="CreateSnippetRequest.Files" /> / <see cref="UpdateSnippetRequest.Files" /> is the
///         form GitLab prefers. The single-file <c>content</c> + <c>file_name</c> pair is the legacy form,
///         still accepted, still mutually exclusive with <c>files</c>, and unable to express a snippet
///         with more than one file.
///     </para>
///     <para>
///         Comments, threads and reactions on a snippet are deliberately not here: they live on
///         <see cref="INotesClient" />, <see cref="IDiscussionsClient" /> and
///         <see cref="IAwardEmojiClient" /> respectively, which model those resources across every type
///         they attach to.
///     </para>
/// </summary>
public interface ISnippetsClient
{
    /// <summary>Streams the authenticated user's own snippets (<c>GET /snippets</c>).</summary>
    IAsyncEnumerable<GitLabSnippet> ListAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every snippet on the instance, personal and project alike (<c>GET /snippets/all</c>).
    ///     Requires Administrator or Auditor access; any other token sees only what it could see anyway.
    /// </summary>
    IAsyncEnumerable<GitLabSnippet> ListAllAsync(AllSnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every public snippet visible to the caller (<c>GET /snippets/public</c>).</summary>
    IAsyncEnumerable<GitLabSnippet> ListPublicAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one snippet by id. Personal and project snippets share a single id space.</summary>
    Task<GitLabSnippet> GetAsync(long snippetId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a personal snippet. Pass <see cref="CreateSnippetRequest.Files" /> for anything but a
    ///     single file; visibility defaults to <see cref="GitLabVisibility.Internal" /> on this endpoint.
    /// </summary>
    Task<GitLabSnippet> CreateAsync(CreateSnippetRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a personal snippet. Only the properties you set are sent, so unset ones are left alone.
    ///     A multi-file snippet must be updated through <see cref="UpdateSnippetRequest.Files" />.
    /// </summary>
    Task<GitLabSnippet> UpdateAsync(long snippetId, UpdateSnippetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a personal snippet. GitLab answers with an empty body, so nothing is deserialized -
    ///     the snippet body the spec declares for this operation is not actually sent.
    /// </summary>
    Task DeleteAsync(long snippetId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the raw content of a snippet as plain text (<c>GET /snippets/:id/raw</c>) - the body
    ///     is bytes, not JSON, and is streamed rather than buffered.
    /// </summary>
    /// <param name="snippetId">The snippet id.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must dispose it - <c>await using</c> - because it holds
    ///     the HTTP response and the pooled connection open until then.
    /// </returns>
    Task<GitLabFileResponse> GetRawAsync(long snippetId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file of a multi-file snippet as plain text
    ///     (<c>GET /snippets/:id/files/:ref/:file_path/raw</c>).
    /// </summary>
    /// <param name="snippetId">The snippet id.</param>
    /// <param name="refName">A branch, tag or commit in the snippet repository - usually <c>main</c>.</param>
    /// <param name="filePath">
    ///     The file path inside the snippet repository, such as <c>src/App.cs</c>. Pass it raw: the route
    ///     builder percent-encodes it, so its slashes stay one path segment.
    /// </param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must dispose it - <c>await using</c> - because it holds
    ///     the HTTP response and the pooled connection open until then.
    /// </returns>
    Task<GitLabFileResponse> GetRawFileAsync(long snippetId, string refName, string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the user-agent details recorded against a snippet for spam checking. Administrator only -
    ///     a non-admin token gets a <see cref="Exceptions.GitLabNotFoundException" />.
    /// </summary>
    Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(long snippetId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every snippet on a project (<c>GET /projects/:id/snippets</c>).</summary>
    IAsyncEnumerable<GitLabSnippet> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every snippet on a project while controlling the number of results fetched in each
    ///     request (<c>GET /projects/:id/snippets?per_page=…</c>). The returned sequence still follows
    ///     GitLab's pagination links to completion.
    /// </summary>
    /// <param name="projectId">The project's numeric id or namespaced path.</param>
    /// <param name="options">The optional per-page payload-size setting declared by GitLab.</param>
    /// <param name="cancellationToken">Cancels enumeration between pages.</param>
    IAsyncEnumerable<GitLabSnippet> ListForProjectAsync(ProjectId projectId,
        ProjectSnippetListOptions options, CancellationToken cancellationToken = default);

    /// <summary>Gets one project snippet.</summary>
    Task<GitLabSnippet> GetForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project snippet. Unlike the personal endpoint this one requires
    ///     <see cref="CreateSnippetRequest.Visibility" />; omitting it is a
    ///     <see cref="Exceptions.GitLabValidationException" />.
    /// </summary>
    Task<GitLabSnippet> CreateForProjectAsync(ProjectId projectId, CreateSnippetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project snippet. A multi-file snippet must be updated through
    ///     <see cref="UpdateSnippetRequest.Files" />.
    /// </summary>
    Task<GitLabSnippet> UpdateForProjectAsync(ProjectId projectId, long snippetId, UpdateSnippetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a project snippet.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, long snippetId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the raw content of a project snippet as plain text
    ///     (<c>GET /projects/:id/snippets/:snippet_id/raw</c>).
    /// </summary>
    /// <param name="projectId">The project the snippet belongs to.</param>
    /// <param name="snippetId">The snippet id.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must dispose it - <c>await using</c> - because it holds
    ///     the HTTP response and the pooled connection open until then.
    /// </returns>
    Task<GitLabFileResponse> GetRawForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file of a multi-file project snippet as plain text
    ///     (<c>GET /projects/:id/snippets/:snippet_id/files/:ref/:file_path/raw</c>).
    /// </summary>
    /// <param name="projectId">The project the snippet belongs to.</param>
    /// <param name="snippetId">The snippet id.</param>
    /// <param name="refName">A branch, tag or commit in the snippet repository - usually <c>main</c>.</param>
    /// <param name="filePath">
    ///     The file path inside the snippet repository, such as <c>src/App.cs</c>. Pass it raw: the route
    ///     builder percent-encodes it, so its slashes stay one path segment.
    /// </param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must dispose it - <c>await using</c> - because it holds
    ///     the HTTP response and the pooled connection open until then.
    /// </returns>
    Task<GitLabFileResponse> GetRawFileForProjectAsync(ProjectId projectId, long snippetId, string refName,
        string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the user-agent details recorded against a project snippet for spam checking. Administrator
    ///     only - a non-admin token gets a <see cref="Exceptions.GitLabNotFoundException" />.
    /// </summary>
    Task<GitLabUserAgentDetail> GetUserAgentDetailForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);
}