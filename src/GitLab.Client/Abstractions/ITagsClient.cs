using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Tags" API area (<c>/projects/:id/repository/tags</c>).</summary>
public interface ITagsClient
{
    IAsyncEnumerable<GitLabTag> ListAsync(ProjectId projectId, TagListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTag> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabTag> CreateAsync(ProjectId projectId, CreateTagRequest request,
        CancellationToken cancellationToken = default);

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