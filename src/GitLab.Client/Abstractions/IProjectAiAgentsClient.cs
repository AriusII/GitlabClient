using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab external AI agent API area (<c>/projects/:id/ai_agent</c>) - the record of coding
///     agents such as Claude Code running against a project, and the audit trail of what they did.
///     <para>
///         The three concepts nest: an <em>identity</em> ties one agent on one machine to one user and
///         project, a <em>session</em> is a single run opened under an identity, and <em>audit events</em>
///         are the batched trail of actions inside a session.
///     </para>
///     <para>
///         Both registration calls are idempotent by design, so a client that retries after a network
///         failure gets back the record it already created rather than a duplicate.
///     </para>
/// </summary>
public interface IProjectAiAgentsClient
{
    /// <summary>
    ///     Registers an agent identity for the calling user, this project, the given agent type and machine
    ///     fingerprint. Returns the existing identity when one already matches all four.
    /// </summary>
    /// <exception cref="Exceptions.GitLabForbiddenException">The matching identity has been revoked.</exception>
    Task<GitLabAgentIdentity> RegisterIdentityAsync(ProjectId projectId, RegisterAgentIdentityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the project's external agent sessions, newest first.</summary>
    IAsyncEnumerable<GitLabAgentSession> ListSessionsAsync(ProjectId projectId,
        AgentSessionListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Opens an external agent session. Set
    ///     <see cref="CreateAgentSessionRequest.IdempotencyKey" /> to make a retry return the session that
    ///     was already created instead of opening a second one.
    /// </summary>
    Task<GitLabAgentSession> CreateSessionAsync(ProjectId projectId, CreateAgentSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves a session to its terminal status, completed or failed. Only the user who created the
    ///     session may close it, and re-sending the transcript digest GitLab already stored returns the
    ///     session unchanged.
    /// </summary>
    Task<GitLabAgentSession> CompleteSessionAsync(ProjectId projectId, long sessionId,
        CompleteAgentSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Submits a batch of audit events for one session - at most 500 per call. GitLab validates and
    ///     deduplicates the batch, then queues it, so this returns as soon as the batch is accepted rather
    ///     than once the events are stored.
    /// </summary>
    Task IngestAuditEventsAsync(ProjectId projectId, IngestAgentAuditEventsRequest request,
        CancellationToken cancellationToken = default);
}