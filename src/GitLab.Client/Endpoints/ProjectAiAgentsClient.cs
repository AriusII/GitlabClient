using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectAiAgentsClient(IGitLabApiConnection connection) : IProjectAiAgentsClient
{
    public Task<GitLabAgentIdentity> RegisterIdentityAsync(ProjectId projectId,
        RegisterAgentIdentityRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            AgentRoute(projectId).Literal("identities").Build(),
            request,
            GitLabJsonContext.Default.RegisterAgentIdentityRequest,
            GitLabJsonContext.Default.GitLabAgentIdentity,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAgentSession> ListSessionsAsync(ProjectId projectId,
        AgentSessionListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            AgentRoute(projectId).Literal("sessions").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabAgentSessionArray,
            cancellationToken);
    }

    public Task<GitLabAgentSession> CreateSessionAsync(ProjectId projectId, CreateAgentSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            AgentRoute(projectId).Literal("sessions").Build(),
            request,
            GitLabJsonContext.Default.CreateAgentSessionRequest,
            GitLabJsonContext.Default.GitLabAgentSession,
            cancellationToken);
    }

    public Task<GitLabAgentSession> CompleteSessionAsync(ProjectId projectId, long sessionId,
        CompleteAgentSessionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            AgentRoute(projectId).Literal("sessions").Segment(sessionId).Build(),
            request,
            GitLabJsonContext.Default.CompleteAgentSessionRequest,
            GitLabJsonContext.Default.GitLabAgentSession,
            cancellationToken);
    }

    public Task IngestAuditEventsAsync(ProjectId projectId, IngestAgentAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            AgentRoute(projectId).Literal("audit_events").Build(),
            request,
            GitLabJsonContext.Default.IngestAgentAuditEventsRequest,
            cancellationToken);
    }

    private static GitLabRouteBuilder AgentRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("ai_agent");
    }
}