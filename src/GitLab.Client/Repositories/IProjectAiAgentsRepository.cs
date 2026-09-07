using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the external AI agent resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectAiAgentsService), typeof(IProjectAiAgentsClient))]
internal interface IProjectAiAgentsRepository
{
    Task<GitLabAgentIdentity> RegisterIdentityAsync(ProjectId projectId, RegisterAgentIdentityRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAgentSession> ListSessionsAsync(ProjectId projectId,
        AgentSessionListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabAgentSession> CreateSessionAsync(ProjectId projectId, CreateAgentSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabAgentSession> CompleteSessionAsync(ProjectId projectId, long sessionId,
        CompleteAgentSessionRequest request, CancellationToken cancellationToken = default);

    Task IngestAuditEventsAsync(ProjectId projectId, IngestAgentAuditEventsRequest request,
        CancellationToken cancellationToken = default);
}