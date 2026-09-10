using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab group credentials inventory API (<c>/groups/:id/manage/...</c>) - a
///     compliance report of every personal access token, group/project access token and SSH key
///     belonging to the group's enterprise users, with the ability to revoke or rotate any of them.
///     Requires the group to be on a top-level namespace with a paid plan and the caller to be an
///     Owner or administrator.
///     <para>
///         Every entry describes a credential that may belong to someone other than the caller. Treat
///         every field a method here returns - names, scopes, expiry, last-used timestamps, and above
///         all the plaintext secret <see cref="RotatePersonalAccessTokenAsync" /> hands back - as
///         sensitive: never log a response verbatim, and persist a rotated secret immediately, since
///         GitLab discloses it exactly once.
///     </para>
/// </summary>
public interface IGroupCredentialsInventoryClient
{
    /// <summary>Streams every personal access token belonging to the group's enterprise users.</summary>
    IAsyncEnumerable<GitLabPersonalAccessToken> ListPersonalAccessTokensAsync(GroupId groupId,
        PersonalAccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Revokes a personal access token belonging to one of the group's enterprise users.</summary>
    Task RevokePersonalAccessTokenAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a personal access token belonging to one of the group's enterprise users, invalidating
    ///     it and returning a replacement with a freshly disclosed secret.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> RotatePersonalAccessTokenAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams every group and project access token within the group's hierarchy.</summary>
    IAsyncEnumerable<GitLabAccessToken> ListResourceAccessTokensAsync(GroupId groupId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes a group or project access token within the group's hierarchy. <paramref name="expiresAt" />
    ///     optionally schedules the revocation for a future date instead of taking effect immediately.
    /// </summary>
    Task RevokeResourceAccessTokenAsync(GroupId groupId, long tokenId, DateOnly? expiresAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a group or project access token within the group's hierarchy, invalidating it and
    ///     issuing a replacement. Unlike <see cref="RotatePersonalAccessTokenAsync" />, GitLab answers with
    ///     no content here - the new token's secret is not returned by this endpoint.
    /// </summary>
    Task RotateResourceAccessTokenAsync(GroupId groupId, long tokenId, RotateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every SSH key belonging to the group's enterprise users.</summary>
    IAsyncEnumerable<GitLabGroupManagedSshKey> ListSshKeysAsync(GroupId groupId,
        GroupManagedSshKeyListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an SSH key belonging to one of the group's enterprise users.</summary>
    Task DeleteSshKeyAsync(GroupId groupId, long keyId, CancellationToken cancellationToken = default);
}