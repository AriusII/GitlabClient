using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Releases" API area (<c>/projects/:id/releases</c>, <c>/groups/:id/releases</c>
///     and the asset links nested under <c>/releases/:tag_name/assets/links</c>).
/// </summary>
public interface IReleasesClient
{
    /// <summary>Lists every release in a project, newest <c>released_at</c> first.</summary>
    IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every release across the projects of a group (<c>GET /groups/:id/releases</c>).
    /// </summary>
    IAsyncEnumerable<GitLabRelease> ListForGroupAsync(GroupId groupId, GroupReleaseListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one release by its Git tag.</summary>
    Task<GitLabRelease> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    /// <summary>Creates a release (<c>POST /projects/:id/releases</c>).</summary>
    Task<GitLabRelease> CreateAsync(ProjectId projectId, CreateReleaseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a release (<c>PUT /projects/:id/releases/:tag_name</c>). Only the fields set on
    ///     <paramref name="request" /> are sent.
    /// </summary>
    Task<GitLabRelease> UpdateAsync(ProjectId projectId, string tagName, UpdateReleaseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a release, leaving its Git tag in place.</summary>
    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Collects a fresh evidence snapshot for an existing release
    ///     (<c>POST /projects/:id/releases/:tag_name/evidence</c>) and returns the updated release.
    /// </summary>
    Task<GitLabRelease> GenerateEvidenceAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the asset links attached to a release.</summary>
    IAsyncEnumerable<GitLabReleaseLink> ListLinksAsync(ProjectId projectId, string tagName,
        ReleaseLinkListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one asset link of a release.</summary>
    Task<GitLabReleaseLink> GetLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);

    /// <summary>Attaches an asset link to a release.</summary>
    Task<GitLabReleaseLink> CreateLinkAsync(ProjectId projectId, string tagName, CreateReleaseLinkRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one asset link of a release. Only the fields set on <paramref name="request" /> are sent.</summary>
    Task<GitLabReleaseLink> UpdateLinkAsync(ProjectId projectId, string tagName, long linkId,
        UpdateReleaseLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes one asset link from a release. GitLab answers with the deleted link; the transport has no
    ///     <c>DELETE</c> overload that reads a response body, so it is not surfaced.
    /// </summary>
    Task DeleteLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);
}