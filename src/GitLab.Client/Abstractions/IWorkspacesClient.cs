using System.Text.Json;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Workspaces" API area's internal agent endpoints
///     (<c>/internal/agents/agentw/...</c>) - the remote-development agent GitLab Workspaces runs
///     alongside a workspace. These back the agent itself rather than an end-user integration, and
///     GitLab's spec declares no response schema for either operation, so both answer with a raw
///     <see cref="JsonElement" />.
/// </summary>
public interface IWorkspacesClient
{
    /// <summary>
    ///     Retrieves agent info for the workspace agent (<c>GET /internal/agents/agentw/agent_info</c>),
    ///     identified by the caller's workspace token.
    /// </summary>
    Task<JsonElement> GetAgentInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user is authorized to access a workspace
    ///     (<c>GET /internal/agents/agentw/authorize_user_access</c>).
    /// </summary>
    /// <param name="workspaceHost">The host of the workspace being accessed.</param>
    /// <param name="userId">The ID of the user requesting access.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<JsonElement> AuthorizeUserAccessAsync(string workspaceHost, long userId,
        CancellationToken cancellationToken = default);
}