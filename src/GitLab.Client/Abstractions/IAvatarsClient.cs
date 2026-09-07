using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Avatars" API area - looking up a user's avatar by public email
///     (<c>/avatar</c>) and downloading the image behind a project's or a group's avatar
///     (<c>/projects/:id/avatar</c>, <c>/groups/:id/avatar</c>).
///     <para>
///         Uploading the authenticated user's own avatar is the fourth operation GitLab files under this
///         tag, and lives on <see cref="ICurrentUserClient.SetAvatarAsync" /> instead, with the rest of the
///         <c>/user</c> surface.
///     </para>
/// </summary>
public interface IAvatarsClient
{
    /// <summary>
    ///     Resolves the avatar URL for a public email address.
    ///     <para>
    ///         This always answers <c>200</c>: when no user on the instance has that public email, GitLab
    ///         still returns the URL the configured avatar service (Gravatar by default) would serve for it.
    ///         A non-null result is therefore not evidence that the account exists.
    ///     </para>
    /// </summary>
    /// <param name="email">The user's <em>public</em> email address, not their sign-in address.</param>
    /// <param name="size">
    ///     The requested single-pixel dimension of the image, or null for the service default. Only
    ///     meaningful for Gravatar-style URLs.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabAvatar> GetForEmailAsync(string email, int? size = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a project's avatar image. Public projects answer without a credential.
    ///     <para>
    ///         The returned <see cref="GitLabFileResponse" /> owns the open body: <c>await using</c> it, and
    ///         read <see cref="GitLabFileResponse.Content" /> before it is disposed. A project with no
    ///         avatar of its own answers <see cref="Exceptions.GitLabNotFoundException" /> rather than
    ///         serving the generated identicon the web UI shows.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a group's avatar image. The returned <see cref="GitLabFileResponse" /> owns the open
    ///     body and must be disposed by the caller.
    /// </summary>
    Task<GitLabFileResponse> DownloadForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}