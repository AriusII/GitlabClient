using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for SSH keys: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISshKeysService), typeof(ISshKeysClient))]
internal interface ISshKeysRepository
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