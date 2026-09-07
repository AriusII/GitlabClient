using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Composer registry - the group-scoped metadata endpoints Composer itself reads
///     (<c>/group/:id/-/packages/composer/...</c>) plus the project-scoped publish and archive-download
///     endpoints (<c>/projects/:id/packages/composer/...</c>).
///     <para>
///         GitLab's own routes put the group endpoints under the singular <c>/group/:id</c>, not the
///         <c>/groups/:id</c> every other group resource in this library uses. That is not a typo here
///         either - it is what the pinned OpenAPI spec documents.
///     </para>
///     <para>
///         Every read here returns a <see cref="GitLabFileResponse" /> rather than a typed DTO: the
///         metadata endpoints answer with Composer's own JSON shapes (provider includes, <c>p2</c>
///         package version metadata), which GitLab's spec does not document a schema for and which this
///         library does not attempt to model - the caller parses the body as Composer itself would.
///     </para>
/// </summary>
public interface IPackagesComposerClient
{
    /// <summary>Gets the repository URL templates Composer uses to resolve individual packages in the group.</summary>
    Task<GitLabFileResponse> GetRepositoryUrlTemplatesForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every package in the group (Composer v1's "provider includes" file, keyed by
    ///     <paramref name="sha" />, the checksum of the current package list).
    /// </summary>
    Task<GitLabFileResponse> ListAllForGroupAsync(GroupId groupId, string sha,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a package's version metadata via the Composer v2 <c>p2</c> endpoint. Prefer this over the v1 form.</summary>
    Task<GitLabFileResponse> GetPackageVersionsV2ForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a package's version metadata via the Composer v1 endpoint.</summary>
    Task<GitLabFileResponse> GetPackageVersionsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Publishes a Composer package from a Git branch or tag. Pass exactly one of
    ///     <see cref="ComposerPackageCreateRequest.Branch" /> or <see cref="ComposerPackageCreateRequest.Tag" />
    ///     on <paramref name="request" />, or omit it to let GitLab use its own default ref.
    /// </summary>
    Task CreateAsync(ProjectId projectId, ComposerPackageCreateRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a package archive. <paramref name="sha" /> is the checksum from the package's metadata.</summary>
    Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId, string packageName, string sha,
        CancellationToken cancellationToken = default);
}