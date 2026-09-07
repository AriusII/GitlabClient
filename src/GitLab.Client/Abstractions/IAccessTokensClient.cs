using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Access tokens" API area for resource-scoped tokens
///     (<c>/projects/:id/access_tokens</c> and <c>/groups/:id/access_tokens</c>).
///     <para>
///         The project and group routes are identical apart from their root segment, but their
///         identifiers are different domain types, so each verb appears once per scope rather than
///         behind a shared "resource kind" discriminator.
///     </para>
///     <para>
///         Read operations return <see cref="GitLabAccessToken" />, which has no secret member at all.
///         Create and rotate return <see cref="GitLabAccessTokenWithSecret" />, because those are the
///         only calls GitLab ever answers with the plaintext token - so the return type of a method
///         tells you whether it can hand you a credential.
///     </para>
///     <para>
///         Personal access tokens, impersonation tokens and service-account tokens live on
///         <see cref="IPersonalAccessTokensClient" />.
///     </para>
/// </summary>
public interface IAccessTokensClient
{
    /// <summary>Lists the access tokens of a project, streaming every page.</summary>
    IAsyncEnumerable<GitLabAccessToken> ListForProjectAsync(ProjectId projectId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Returns one project access token. Never carries the plaintext secret.</summary>
    Task<GitLabAccessToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project access token. The response is the only time GitLab returns the plaintext
    ///     <see cref="GitLabAccessTokenWithSecret.Token" /> - persist it now or lose it.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> CreateForProjectAsync(ProjectId projectId, CreateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a project access token: the previous token is revoked immediately and the replacement
    ///     is returned with its plaintext <see cref="GitLabAccessTokenWithSecret.Token" /> populated.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> RotateForProjectAsync(ProjectId projectId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates the project access token that authenticated this call
    ///     (<c>POST /projects/:id/access_tokens/self/rotate</c>), without needing to know its ID. The
    ///     token must carry the <c>self_rotate</c> scope, and this client keeps presenting the old,
    ///     now-revoked credential until it is reconfigured with the returned one.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> RotateSelfForProjectAsync(ProjectId projectId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Revokes a project access token.</summary>
    Task RevokeForProjectAsync(ProjectId projectId, long tokenId, CancellationToken cancellationToken = default);

    /// <summary>Lists the access tokens of a group, streaming every page.</summary>
    IAsyncEnumerable<GitLabAccessToken> ListForGroupAsync(GroupId groupId, AccessTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns one group access token. Never carries the plaintext secret.</summary>
    Task<GitLabAccessToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group access token. The response is the only time GitLab returns the plaintext
    ///     <see cref="GitLabAccessTokenWithSecret.Token" /> - persist it now or lose it.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> CreateForGroupAsync(GroupId groupId, CreateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a group access token: the previous token is revoked immediately and the replacement is
    ///     returned with its plaintext <see cref="GitLabAccessTokenWithSecret.Token" /> populated.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> RotateForGroupAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates the group access token that authenticated this call
    ///     (<c>POST /groups/:id/access_tokens/self/rotate</c>), without needing to know its ID. The token
    ///     must carry the <c>self_rotate</c> scope.
    /// </summary>
    Task<GitLabAccessTokenWithSecret> RotateSelfForGroupAsync(GroupId groupId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Revokes a group access token.</summary>
    Task RevokeForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);
}