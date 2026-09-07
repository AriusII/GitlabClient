using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Organizations" API area (<c>/organizations</c>) - the newer top-level container
///     GitLab is building above groups. Introduced in GitLab 17.5 and still experimental as of 19.4, so
///     the spec exposes only creation and soft-delete today.
/// </summary>
public interface IOrganizationsClient
{
    /// <summary>
    ///     Creates an organization (<c>POST /organizations</c>).
    ///     <para>
    ///         GitLab documents this endpoint as <c>multipart/form-data</c> so the optional avatar can ride
    ///         along with it - there is no separate avatar route the way there is for groups and projects.
    ///         Pass <paramref name="avatar" /> to attach one; the stream is read but not disposed. Leaving
    ///         it null - the common case - sends <paramref name="request" /> as a plain JSON body instead,
    ///         since there is then no file part to include.
    ///     </para>
    /// </summary>
    Task<GitLabOrganization> CreateAsync(CreateOrganizationRequest request, GitLabFileUpload? avatar = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Soft-deletes an organization (<c>DELETE /organizations/:id</c>). GitLab answers
    ///     <c>202 Accepted</c>; the spec exposes no restore endpoint yet.
    /// </summary>
    Task DeleteAsync(long organizationId, CancellationToken cancellationToken = default);
}