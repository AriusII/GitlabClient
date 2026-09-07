using System.Text.Json;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Workspaces, sitting between the public
///     <c>IWorkspacesClient</c> controller and <c>IWorkspacesRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once this resource
///     needs more than pass-through.
/// </summary>
internal interface IWorkspacesService
{
    Task<JsonElement> GetAgentInfoAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> AuthorizeUserAccessAsync(string workspaceHost, long userId,
        CancellationToken cancellationToken = default);
}