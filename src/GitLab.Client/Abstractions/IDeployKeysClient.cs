using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Deploy keys" API area (<c>/projects/:id/deploy_keys</c>).</summary>
public interface IDeployKeysClient
{
    /// <summary>Streams the deploy keys attached to one project.</summary>
    IAsyncEnumerable<GitLabDeployKey> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Gets one of a project's deploy keys by ID.</summary>
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

    /// <summary>Removes a deploy key from the project. If no other project uses it, GitLab deletes it entirely.</summary>
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
    ///     Creates a deploy key directly on the GitLab instance from the legacy project-shaped request. Its
    ///     project-only push permission is discarded before the request is sent. Prefer
    ///     <see cref="CreateForInstanceAsync" /> for new code.
    /// </summary>
    Task<GitLabDeployKey> CreateAsync(CreateDeployKeyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a deploy key directly on the GitLab instance (<c>POST /deploy_keys</c>), rather than on a
    ///     project. Requires administrator access. Unlike <see cref="AddAsync" />, the created key is not
    ///     attached to any project until <see cref="EnableAsync" /> grants one access to it.
    /// </summary>
    Task<GitLabDeployKey> CreateForInstanceAsync(CreateInstanceDeployKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return CreateAsync(
            new CreateDeployKeyRequest { Key = request.Key, Title = request.Title, ExpiresAt = request.ExpiresAt },
            cancellationToken);
    }

    /// <summary>
    ///     Streams every project deploy key accessible to a user (<c>GET /users/:user_id/project_deploy_keys</c>),
    ///     across every project that user can access. Requires administrator access.
    /// </summary>
    /// <param name="userId">The user's numeric ID.</param>
    /// <param name="options">Optional pagination settings.</param>
    /// <param name="cancellationToken">Cancels the enumeration between pages.</param>
    IAsyncEnumerable<GitLabDeployKey> ListForUserAsync(long userId,
        UserProjectDeployKeyListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every project deploy key accessible to a user (<c>GET /users/:user_id/project_deploy_keys</c>),
    ///     addressing that user by username rather than numeric ID. Requires administrator access.
    /// </summary>
    /// <param name="userId">The username accepted by GitLab's <c>user_id</c> route segment.</param>
    /// <param name="options">Optional pagination settings.</param>
    /// <param name="cancellationToken">Cancels the enumeration between pages.</param>
    IAsyncEnumerable<GitLabDeployKey> ListForUserAsync(string userId,
        UserProjectDeployKeyListOptions? options = null, CancellationToken cancellationToken = default);
}