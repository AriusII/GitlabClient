using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Workspaces resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Both operations are internal, agent-facing endpoints (<c>/internal/agents/agentw/...</c>) that
///         back GitLab Workspaces' remote-development agent. Neither declares a response schema in the
///         spec at all - not even an untyped "object" - so both answer with a raw <see cref="JsonElement" />
///         rather than an invented DTO, the same pattern already used for the Duo/AI endpoints spec leaves
///         equally undocumented.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IWorkspacesService), typeof(IWorkspacesClient))]
internal interface IWorkspacesRepository
{
    Task<JsonElement> GetAgentInfoAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> AuthorizeUserAccessAsync(string workspaceHost, long userId,
        CancellationToken cancellationToken = default);
}