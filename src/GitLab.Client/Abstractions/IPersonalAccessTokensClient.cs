using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Access tokens" API area for user-scoped tokens: personal access tokens
///     (<c>/personal_access_tokens</c>, <c>/user/personal_access_tokens</c>,
///     <c>/users/:user_id/personal_access_tokens</c>), the personal access tokens of a project or group
///     service account (<c>/projects/:id/service_accounts/:user_id/personal_access_tokens</c> and its
///     group form), and impersonation tokens (<c>/users/:user_id/impersonation_tokens</c>).
///     <para>
///         Read operations return <see cref="GitLabPersonalAccessToken" /> or
///         <see cref="GitLabImpersonationToken" />, neither of which has a secret member at all.
///         Create and rotate return the matching <c>...WithSecret</c> type, because those are the only
///         calls GitLab ever answers with the plaintext token - so the return type of a method tells
///         you whether it can hand you a credential, and the secret cannot be smuggled through a
///         listing.
///     </para>
///     <para>
///         Three different senses of "the current identity" appear in these routes and they are not
///         interchangeable: <c>/personal_access_tokens/self</c> means
///         <em>
///             the token that authenticated
///             this call
///         </em>
///         (<see cref="GetSelfAsync" />, <see cref="RevokeSelfAsync" />,
///         <see cref="RotateSelfAsync" />, <see cref="GetSelfAssociationsAsync" />),
///         <c>/user/personal_access_tokens</c> means <em>the user who owns that token</em>
///         (<see cref="CreateForCurrentUserAsync" />), and <c>/users/:user_id/...</c> is the
///         administrator route that targets somebody else.
///     </para>
///     <para>
///         Project- and group-scoped access tokens live on <see cref="IAccessTokensClient" />.
///     </para>
/// </summary>
public interface IPersonalAccessTokensClient
{
    /// <summary>
    ///     Lists personal access tokens, streaming every page. Administrators see every token on the
    ///     instance; everyone else sees only their own.
    /// </summary>
    IAsyncEnumerable<GitLabPersonalAccessToken> ListAsync(PersonalAccessTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns one personal access token. Never carries the plaintext secret.</summary>
    Task<GitLabPersonalAccessToken> GetAsync(long tokenId, CancellationToken cancellationToken = default);

    /// <summary>Revokes a personal access token.</summary>
    Task RevokeAsync(long tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a personal access token: the previous token is revoked immediately and the replacement
    ///     is returned with its plaintext <see cref="GitLabPersonalAccessTokenWithSecret.Token" />
    ///     populated.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> RotateAsync(long tokenId, RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the token that authenticated this call. Only meaningful when the client is configured
    ///     with a personal access token - under an OAuth or job token GitLab answers 401.
    /// </summary>
    Task<GitLabPersonalAccessToken> GetSelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes the token that authenticated this call, so every subsequent call from this client
    ///     fails. Only meaningful when the client is configured with a personal access token.
    /// </summary>
    Task RevokeSelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates the token that authenticated this call. The replacement is returned with its plaintext
    ///     <see cref="GitLabPersonalAccessTokenWithSecret.Token" /> populated, and the client keeps using
    ///     the old, now-revoked credential until it is reconfigured. The token must carry the
    ///     <c>self_rotate</c> scope.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> RotateSelfAsync(RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the groups and projects the authenticating token can reach
    ///     (<c>GET /personal_access_tokens/self/associations</c>) - normally everything its owner is a
    ///     member of, narrowed by <see cref="TokenAssociationListOptions.MinAccessLevel" />. Unlike the
    ///     other listings here this route answers with a single envelope object rather than a JSON array,
    ///     so it returns one <see cref="GitLabTokenAssociations" /> and pages explicitly.
    /// </summary>
    Task<GitLabTokenAssociations> GetSelfAssociationsAsync(TokenAssociationListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a personal access token for the currently authenticated user
    ///     (<c>POST /user/personal_access_tokens</c>). GitLab deliberately narrows this route to the
    ///     <c>k8s_proxy</c> and <c>self_rotate</c> scopes. The response is the only time GitLab returns
    ///     the plaintext <see cref="GitLabPersonalAccessTokenWithSecret.Token" /> - persist it now or
    ///     lose it.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> CreateForCurrentUserAsync(
        CreateCurrentUserPersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a personal access token for another user. Administrators only. The response is the
    ///     only time GitLab returns the plaintext
    ///     <see cref="GitLabPersonalAccessTokenWithSecret.Token" />.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> CreateForUserAsync(long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the personal access tokens of a project service account, streaming every page.
    ///     <paramref name="userId" /> is the service account user, not the caller.
    /// </summary>
    IAsyncEnumerable<GitLabPersonalAccessToken> ListForProjectServiceAccountAsync(ProjectId projectId, long userId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a personal access token for a project service account. The response is the only time
    ///     GitLab returns the plaintext <see cref="GitLabPersonalAccessTokenWithSecret.Token" />.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> CreateForProjectServiceAccountAsync(ProjectId projectId, long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a project service account token: the previous token is revoked immediately and the
    ///     replacement is returned with its plaintext secret populated.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> RotateForProjectServiceAccountAsync(ProjectId projectId, long userId,
        long tokenId, RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Revokes a project service account token.</summary>
    Task RevokeForProjectServiceAccountAsync(ProjectId projectId, long userId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the personal access tokens of a group service account, streaming every page.
    ///     <paramref name="userId" /> is the service account user, not the caller.
    /// </summary>
    IAsyncEnumerable<GitLabPersonalAccessToken> ListForGroupServiceAccountAsync(GroupId groupId, long userId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a personal access token for a group service account. The response is the only time
    ///     GitLab returns the plaintext <see cref="GitLabPersonalAccessTokenWithSecret.Token" />.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> CreateForGroupServiceAccountAsync(GroupId groupId, long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a group service account token: the previous token is revoked immediately and the
    ///     replacement is returned with its plaintext secret populated.
    /// </summary>
    Task<GitLabPersonalAccessTokenWithSecret> RotateForGroupServiceAccountAsync(GroupId groupId, long userId,
        long tokenId, RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Revokes a group service account token.</summary>
    Task RevokeForGroupServiceAccountAsync(GroupId groupId, long userId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a user's impersonation tokens, streaming every page. Administrators only; the tokens are
    ///     invisible to the user they impersonate.
    /// </summary>
    IAsyncEnumerable<GitLabImpersonationToken> ListImpersonationTokensAsync(long userId,
        ImpersonationTokenListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Returns one impersonation token. Administrators only. Never carries the plaintext secret.</summary>
    Task<GitLabImpersonationToken> GetImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an impersonation token that acts as <paramref name="userId" /> for both API calls and
    ///     Git reads and writes. Administrators only, and the most privileged credential this library can
    ///     return - the response is the only time GitLab discloses
    ///     <see cref="GitLabImpersonationTokenWithSecret.Token" />.
    /// </summary>
    Task<GitLabImpersonationTokenWithSecret> CreateImpersonationTokenAsync(long userId,
        CreateImpersonationTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Revokes an impersonation token. Administrators only.</summary>
    Task RevokeImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default);
}