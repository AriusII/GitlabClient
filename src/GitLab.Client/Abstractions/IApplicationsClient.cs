using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's OAuth application registration surface: the instance-wide admin area
///     (<c>/applications</c>) and the per-user area (<c>/user/applications</c>) that lets any user
///     register their own OAuth application.
///     <para>
///         GitLab discloses an application's client secret exactly once per credential - on
///         <see cref="CreateAsync" />/<see cref="CreateForCurrentUserAsync" /> and again on
///         <see cref="RenewSecretAsync" /> - which is why those three methods return
///         <see cref="GitLabApplicationWithSecret" /> while every other method returns the secret-free
///         <see cref="GitLabApplication" />. Persist a returned secret immediately; GitLab cannot show
///         it again, and callers must never log the create/renew request or response.
///     </para>
///     <para>
///         <see cref="GetWorkspacesHttpServerConfigAsync" /> is the odd one out: GitLab's spec tags it
///         "OAuth applications" even though it carries the internal, agent-facing configuration for
///         GitLab Workspaces' HTTP server rather than anything about OAuth clients. It is exposed here
///         only because that is its spec tag; do not read it as part of the application-registration
///         surface above.
///     </para>
/// </summary>
public interface IApplicationsClient
{
    /// <summary>Streams every OAuth application registered instance-wide. Administrators only.</summary>
    IAsyncEnumerable<GitLabApplication> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Registers a new instance-wide OAuth application, returning it together with its plaintext
    ///     client secret. Administrators only.
    /// </summary>
    Task<GitLabApplicationWithSecret> CreateAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an instance-wide OAuth application. Administrators only.</summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Issues a new client secret for an instance-wide OAuth application, invalidating the old one,
    ///     and returns the application with the freshly issued plaintext secret. Administrators only.
    /// </summary>
    Task<GitLabApplicationWithSecret> RenewSecretAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Streams every OAuth application the current user has registered for themselves.</summary>
    IAsyncEnumerable<GitLabApplication> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Registers a new OAuth application owned by the current user, returning it together with its
    ///     plaintext client secret.
    /// </summary>
    Task<GitLabApplicationWithSecret> CreateForCurrentUserAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one of the current user's OAuth applications by ID.</summary>
    Task<GitLabApplication> GetForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Updates the name and/or scopes of one of the current user's OAuth applications.</summary>
    Task<GitLabApplication> UpdateForCurrentUserAsync(long id, UpdateApplicationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one of the current user's OAuth applications.</summary>
    Task DeleteForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns configuration for the GitLab Workspaces HTTP server
    ///     (<c>GET /internal/agents/agentw/server_config</c>). An internal, agent-facing endpoint - see the
    ///     remarks on this interface for why it lives here despite the name. GitLab's spec declares no
    ///     response schema, so the answer is a raw <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    Task<JsonElement> GetWorkspacesHttpServerConfigAsync(CancellationToken cancellationToken = default);
}