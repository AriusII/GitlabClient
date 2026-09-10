using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Deploy tokens" API area (<c>/deploy_tokens</c>,
///     <c>/projects/:id/deploy_tokens</c>, <c>/groups/:id/deploy_tokens</c>) - the username/password
///     credentials that let a machine clone a repository and pull from the container and package
///     registries without belonging to a user.
///     <para>
///         Deploy <em>tokens</em> are not deploy <em>keys</em>: keys are SSH public keys and live on
///         <see cref="IDeployKeysClient" />. The two resources share nothing but a name prefix.
///     </para>
///     <para>
///         <b>The password is disclosed exactly once.</b> Only the two create calls can return it, and
///         they say so in their type: they answer with <see cref="GitLabDeployTokenWithSecret" />,
///         while every read returns the secret-free <see cref="GitLabDeployToken" />. Persist
///         <see cref="GitLabDeployTokenWithSecret.Token" /> immediately - GitLab cannot show it again,
///         and a lost token can only be replaced.
///     </para>
/// </summary>
public interface IDeployTokensClient
{
    /// <summary>
    ///     Streams every deploy token on the instance. Administrators only; a non-administrator gets a
    ///     <see cref="Exceptions.GitLabForbiddenException" />.
    /// </summary>
    IAsyncEnumerable<GitLabDeployToken> ListAsync(DeployTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams a project's deploy tokens. Requires at least the Maintainer role on the project.</summary>
    IAsyncEnumerable<GitLabDeployToken> ListForProjectAsync(ProjectId projectId,
        DeployTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one of a project's deploy tokens by ID.</summary>
    Task<GitLabDeployToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a deploy token on a project and returns it <b>with its plaintext password</b> - the
    ///     only moment GitLab discloses it. Store
    ///     <see cref="GitLabDeployTokenWithSecret.Token" /> before the value goes out of scope.
    /// </summary>
    Task<GitLabDeployTokenWithSecret> CreateForProjectAsync(ProjectId projectId, CreateDeployTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a project's deploy tokens, revoking the credential immediately.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, long tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a group's deploy tokens. A group deploy token works against every project in the
    ///     group, including projects added later.
    /// </summary>
    IAsyncEnumerable<GitLabDeployToken> ListForGroupAsync(GroupId groupId, DeployTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one of a group's deploy tokens by ID.</summary>
    Task<GitLabDeployToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a deploy token on a group and returns it <b>with its plaintext password</b> - the only
    ///     moment GitLab discloses it. Store <see cref="GitLabDeployTokenWithSecret.Token" /> before the
    ///     value goes out of scope.
    /// </summary>
    Task<GitLabDeployTokenWithSecret> CreateForGroupAsync(GroupId groupId, CreateDeployTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a group's deploy tokens, revoking the credential immediately.</summary>
    Task DeleteForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);
}