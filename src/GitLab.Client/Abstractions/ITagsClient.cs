using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Tags" API area (<c>/projects/:id/repository/tags</c>).</summary>
public interface ITagsClient
{
    /// <summary>Streams every repository tag for a project (<c>GET /projects/:id/repository/tags</c>).</summary>
    IAsyncEnumerable<GitLabTag> ListAsync(ProjectId projectId, TagListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one repository tag by name (<c>GET /projects/:id/repository/tags/:tag_name</c>). The name is
    ///     percent-encoded, so a tag containing a slash needs no preparation by the caller.
    /// </summary>
    Task<GitLabTag> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a tag from a ref (<c>POST /projects/:id/repository/tags</c>), optionally as an annotated
    ///     tag when <see cref="CreateTagRequest.Message" /> is set.
    /// </summary>
    Task<GitLabTag> CreateAsync(ProjectId projectId, CreateTagRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one repository tag (<c>DELETE /projects/:id/repository/tags/:tag_name</c>). The name is
    ///     percent-encoded, so a tag containing a slash needs no preparation by the caller.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the X.509 signature on a signed tag
    ///     (<c>GET /projects/:id/repository/tags/:tag_name/signature</c>). GitLab answers <c>404</c> for an
    ///     unsigned tag, which surfaces as a <see cref="Exceptions.GitLabNotFoundException" /> rather than
    ///     as a null signature.
    /// </summary>
    Task<GitLabTagSignature> GetSignatureAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);
}