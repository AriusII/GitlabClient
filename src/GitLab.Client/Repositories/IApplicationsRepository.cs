using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Applications resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Also carries <see cref="GetWorkspacesHttpServerConfigAsync" />: GitLab's spec tags
///         <c>GET /internal/agents/agentw/server_config</c> "OAuth applications" even though it has
///         nothing to do with application registration - it is the third member of the internal,
///         agent-facing <c>/internal/agents/agentw/...</c> family that <see cref="IWorkspacesRepository" />
///         otherwise wraps (<c>agent_info</c>, <c>authorize_user_access</c>). It lives here rather than on
///         Workspaces solely because that is the tag this scope owns; see this resource's integration
///         notes for the reasoning.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IApplicationsService), typeof(IApplicationsClient))]
internal interface IApplicationsRepository
{
    IAsyncEnumerable<GitLabApplication> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> RenewSecretAsync(long id, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabApplication> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationWithSecret> CreateForCurrentUserAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApplication> GetForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabApplication> UpdateForCurrentUserAsync(long id, UpdateApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForCurrentUserAsync(long id, CancellationToken cancellationToken = default);

    Task<JsonElement> GetWorkspacesHttpServerConfigAsync(CancellationToken cancellationToken = default);
}