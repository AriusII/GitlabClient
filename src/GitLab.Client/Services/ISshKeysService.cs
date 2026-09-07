using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for SSH keys, sitting between the public <c>ISshKeysClient</c>
///     controller and <c>ISshKeysRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go.
/// </summary>
internal interface ISshKeysService
{
    IAsyncEnumerable<GitLabSshKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<GitLabSshKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    Task<GitLabSshKey> CreateForCurrentUserAsync(CreateSshKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSshKey> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabSshKey> GetForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    Task<GitLabSshKey> CreateForUserAsync(long userId, CreateSshKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    Task<GitLabUser> GetUserByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default);

    Task<GitLabSshKey> GetByIdAsync(long keyId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSshCertificate> ListGroupCertificatesAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabSshCertificate> AddGroupCertificateAsync(GroupId groupId, CreateSshCertificateRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteGroupCertificateAsync(GroupId groupId, long certificateId,
        CancellationToken cancellationToken = default);
}