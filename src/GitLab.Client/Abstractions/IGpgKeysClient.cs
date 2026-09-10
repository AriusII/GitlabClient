using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GPG-key half of the GitLab "Keys" API area (<c>/user/gpg_keys</c> and
///     <c>/users/:id/gpg_keys</c>). GitLab uses these keys to verify commit signatures.
///     <para>
///         Note the one-letter difference between the two route roots: <c>/user/gpg_keys</c> acts on the
///         account the client is authenticated as, while <c>/users/:id/gpg_keys</c> acts on the named
///         user and is administrator-only. The method names spell that out -
///         <c>...ForCurrentUserAsync</c> versus <c>...ForUserAsync</c> - so the two cannot be confused at
///         a call site.
///     </para>
///     <para>
///         Deleting and revoking are different operations, not synonyms: deleting removes the key, while
///         revoking additionally marks every commit signed with it as unverified.
///     </para>
///     <para>SSH keys live on <see cref="ISshKeysClient" />.</para>
/// </summary>
public interface IGpgKeysClient
{
    /// <summary>Lists the authenticated user's GPG keys, streaming every page.</summary>
    IAsyncEnumerable<GitLabGpgKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves one of the authenticated user's GPG keys.</summary>
    Task<GitLabGpgKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>Adds a GPG key to the authenticated user's account.</summary>
    Task<GitLabGpgKey> CreateForCurrentUserAsync(CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a GPG key from the authenticated user's account.</summary>
    Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes one of the authenticated user's GPG keys, which also marks every commit signed with it
    ///     as unverified. Stronger than <see cref="DeleteForCurrentUserAsync" />.
    /// </summary>
    Task RevokeForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>Lists another user's GPG keys, streaming every page.</summary>
    IAsyncEnumerable<GitLabGpgKey> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one of another user's GPG keys.</summary>
    Task<GitLabGpgKey> GetForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>Adds a GPG key to another user's account. Administrators only.</summary>
    Task<GitLabGpgKey> CreateForUserAsync(long userId, CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a GPG key from another user's account. Administrators only.</summary>
    Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes another user's GPG key, which also marks every commit signed with it as unverified.
    ///     Administrators only.
    /// </summary>
    Task RevokeForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);
}