using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Deploy keys" API area (<c>/projects/:id/deploy_keys</c>).</summary>
public interface IDeployKeysClient
{
    /// <summary>Streams the deploy keys attached to one project.</summary>
    IAsyncEnumerable<GitLabDeployKey> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> GetAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new deploy key to the project, creating the key itself.</summary>
    Task<GitLabDeployKey> AddAsync(ProjectId projectId, CreateDeployKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renames a deploy key or changes its push permission. GitLab answers with the plain deploy-key shape,
    ///     so <c>CanPush</c> on the result is null even when the call set it.
    /// </summary>
    Task<GitLabDeployKey> UpdateAsync(ProjectId projectId, long keyId, UpdateDeployKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Grants <em>this</em> project access to a deploy key that already exists on another project the caller
    ///     can administer. It does not create a key - use <see cref="AddAsync" /> for that.
    /// </summary>
    Task<GitLabDeployKey> EnableAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every deploy key on the instance (<c>GET /deploy_keys</c>), which requires administrator
    ///     access. Pass <paramref name="publicOnly" /> as <see langword="true" /> to return only public keys.
    /// </summary>
    IAsyncEnumerable<GitLabDeployKey> ListAllAsync(bool? publicOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a deploy key directly on the GitLab instance (<c>POST /deploy_keys</c>), rather than on a
    ///     project. Requires administrator access. Unlike <see cref="AddAsync" />, the created key is not
    ///     attached to any project until <see cref="EnableAsync" /> grants one access to it.
    /// </summary>
    Task<GitLabDeployKey> CreateAsync(CreateDeployKeyRequest request, CancellationToken cancellationToken = default);
}