using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for external AI agents, sitting between the public
///     <c>IProjectAiAgentsClient</c> controller and <c>IProjectAiAgentsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IProjectAiAgentsService
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