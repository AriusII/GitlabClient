using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the SSH-key half of the GitLab "Keys" API area (<c>/user/keys</c> and
///     <c>/users/:id/keys</c>).
///     <para>
///         Note the one-letter difference between the two route roots: <c>/user/keys</c> acts on the
///         account the client is authenticated as, while <c>/users/:id/keys</c> acts on the named user.
///         The method names spell that out - <c>...ForCurrentUserAsync</c> versus
///         <c>...ForUserAsync</c> - so the two cannot be confused at a call site.
///     </para>
///     <para>
///         The instance-wide lookups <c>/keys</c> and <c>/keys/:id</c> live here too - they resolve any
///         key on the instance to its owner and are administrator-only - as do the group SSH
///         certificate authorities at <c>/groups/:id/ssh_certificates</c>, which share GitLab's "Keys"
///         tag. GPG keys have their own client, <see cref="IGpgKeysClient" />.
///     </para>
/// </summary>
public interface ISshKeysClient
{
    /// <summary>Lists the authenticated user's SSH keys, streaming every page.</summary>
    IAsyncEnumerable<GitLabSshKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves one of the authenticated user's SSH keys by its id.</summary>
    Task<GitLabSshKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>Adds an SSH key to the authenticated user's account.</summary>
    Task<GitLabSshKey> CreateForCurrentUserAsync(CreateSshKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes an SSH key from the authenticated user's account.</summary>
    Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>Lists another user's SSH keys, streaming every page. GitLab serves this without authentication.</summary>
    IAsyncEnumerable<GitLabSshKey> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one of another user's SSH keys. GitLab serves this without authentication.</summary>
    Task<GitLabSshKey> GetForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>Adds an SSH key to another user's account. Administrators only.</summary>
    Task<GitLabSshKey> CreateForUserAsync(long userId, CreateSshKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes an SSH key from another user's account. Administrators only.</summary>
    Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resolves an SSH key fingerprint to the user that owns it (<c>GET /keys?fingerprint=...</c>).
    ///     Administrators only. The fingerprint is sent as a query parameter and percent-encoded, so the
    ///     <c>:</c>, <c>+</c> and <c>/</c> characters a base64 SHA-256 fingerprint contains survive intact.
    /// </summary>
    /// <param name="fingerprint">
    ///     The key fingerprint, either MD5 (<c>9a:2b:...</c>) or SHA-256 (<c>SHA256:base64...</c>).
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabUser> GetUserByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves any SSH key on the instance by its ID (<c>GET /keys/:id</c>), together with its owner
    ///     in <see cref="GitLabSshKey.User" />. Administrators only. Unlike
    ///     <see cref="GetForUserAsync" /> this is not scoped to a user, so it is the lookup to use when
    ///     all you have is the key ID.
    /// </summary>
    Task<GitLabSshKey> GetByIdAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a group's trusted SSH certificate authorities, streaming every page
    ///     (<c>GET /groups/:id/ssh_certificates</c>).
    /// </summary>
    IAsyncEnumerable<GitLabSshCertificate> ListGroupCertificatesAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Registers an SSH certificate authority on a group (<c>POST /groups/:id/ssh_certificates</c>).</summary>
    Task<GitLabSshCertificate> AddGroupCertificateAsync(GroupId groupId, CreateSshCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes an SSH certificate authority from a group
    ///     (<c>DELETE /groups/:id/ssh_certificates/:ssh_certificates_id</c>).
    /// </summary>
    Task DeleteGroupCertificateAsync(GroupId groupId, long certificateId,
        CancellationToken cancellationToken = default);
}