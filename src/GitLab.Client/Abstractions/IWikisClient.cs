using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Wikis" API area (<c>/projects/:id/wikis</c> and <c>/groups/:id/wikis</c>).
///     <para>
///         Comments on wiki pages are notes and are reached through <see cref="INotesClient" />.
///     </para>
///     <para>Group wikis require GitLab Premium; project wikis do not.</para>
/// </summary>
public interface IWikisClient
{
    /// <summary>Streams every page of a project's wiki (<c>GET /projects/:id/wikis</c>).</summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="withContent">
    ///     When true, each page carries its <see cref="GitLabWikiPage.Content" />; GitLab omits it otherwise.
    /// </param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabWikiPage> ListForProjectAsync(ProjectId projectId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one page of a project's wiki (<c>GET /projects/:id/wikis/:slug</c>).</summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="slug">
    ///     The page's slug. A nested page's slug contains '/' (<c>home/setup</c>) and is percent-encoded for
    ///     you, so pass it exactly as GitLab reports it.
    /// </param>
    /// <param name="version">A commit sha to read a historical revision of the page.</param>
    /// <param name="renderHtml">When true, GitLab renders the content to HTML before returning it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabWikiPage> GetForProjectAsync(ProjectId projectId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a page in a project's wiki (<c>POST /projects/:id/wikis</c>). GitLab derives the slug from
    ///     the title, so read it off the returned page.
    /// </summary>
    Task<GitLabWikiPage> CreateForProjectAsync(ProjectId projectId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one page of a project's wiki (<c>PUT /projects/:id/wikis/:slug</c>).</summary>
    Task<GitLabWikiPage> UpdateForProjectAsync(ProjectId projectId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one page of a project's wiki (<c>DELETE /projects/:id/wikis/:slug</c>).</summary>
    Task DeleteForProjectAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a file to a project wiki's <c>uploads</c> directory (<c>POST /projects/:id/wikis/attachments</c>),
    ///     without creating or editing a page - paste the returned markdown into a page body to embed it.
    /// </summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="file">
    ///     The file part. Its stream is read but never disposed, so the caller keeps ownership of it. GitLab
    ///     requires the multipart field name <c>file</c>; any supplied <see cref="GitLabFileUpload.FieldName" /> is ignored.
    /// </param>
    /// <param name="branch">The wiki branch to commit the attachment to. Defaults to the wiki's default branch.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabWikiAttachment> UploadAttachmentForProjectAsync(ProjectId projectId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);

    /// <summary>Streams every page of a group's wiki (<c>GET /groups/:id/wikis</c>). Requires GitLab Premium.</summary>
    IAsyncEnumerable<GitLabWikiPage> ListForGroupAsync(GroupId groupId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one page of a group's wiki (<c>GET /groups/:id/wikis/:slug</c>). Requires GitLab Premium.</summary>
    Task<GitLabWikiPage> GetForGroupAsync(GroupId groupId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a page in a group's wiki (<c>POST /groups/:id/wikis</c>). Requires GitLab Premium.</summary>
    Task<GitLabWikiPage> CreateForGroupAsync(GroupId groupId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one page of a group's wiki (<c>PUT /groups/:id/wikis/:slug</c>). Requires GitLab Premium.</summary>
    Task<GitLabWikiPage> UpdateForGroupAsync(GroupId groupId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one page of a group's wiki (<c>DELETE /groups/:id/wikis/:slug</c>). Requires GitLab Premium.</summary>
    Task DeleteForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a file to a group wiki's <c>uploads</c> directory (<c>POST /groups/:id/wikis/attachments</c>).
    ///     Requires GitLab Premium.
    /// </summary>
    /// <param name="groupId">The group's numeric id or its namespaced path.</param>
    /// <param name="file">
    ///     The file part. Its stream is read but never disposed, so the caller keeps ownership of it. GitLab
    ///     requires the multipart field name <c>file</c>; any supplied <see cref="GitLabFileUpload.FieldName" /> is ignored.
    /// </param>
    /// <param name="branch">The wiki branch to commit the attachment to. Defaults to the wiki's default branch.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabWikiAttachment> UploadAttachmentForGroupAsync(GroupId groupId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);
}