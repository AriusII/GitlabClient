using System.Text.Json;

using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab Duo Agent Platform "flows" API area (<c>/ai/duo_workflows/...</c>) - starting and
///     steering an agentic flow, its LangGraph checkpoints and events, the callback endpoints GitLab
///     delivers flow lifecycle events to, and the Duo Code Review helpers that run inside a flow.
///     <para>
///         <strong>Experimental.</strong> GitLab marks most of these operations
///         <c>x-gitlab-lifecycle: experiment</c> and tags the checkpoint, event, audit-event and
///         status-update routes as <em>internal operations</em> - the contract between GitLab and the Duo
///         Workflow Service rather than a public integration surface. Expect routes, parameters and
///         payloads to change between GitLab minor versions, without deprecation.
///     </para>
///     <para>
///         <strong>Untyped responses.</strong> Apart from the flow callback hooks, the spec declares no
///         response schema for a single operation in this area. Rather than invent DTOs GitLab has not
///         promised - which would silently degrade to all-null objects the moment a field is renamed -
///         those operations hand back the raw <see cref="JsonElement" /> body, so nothing is lost and
///         nothing is guessed. They will be given real DTOs once the spec describes them.
///     </para>
///     <para>
///         Most of these routes need a token with the <c>ai_workflows</c> scope, and the whole area
///         requires GitLab Duo to be available for the namespace; expect
///         <see cref="Exceptions.GitLabForbiddenException" /> or
///         <see cref="Exceptions.GitLabNotFoundException" /> otherwise.
///     </para>
/// </summary>
public interface IDuoWorkflowsClient
{
    /// <summary>
    ///     Streams the flow callback endpoints registered for the organization. Secrets are never
    ///     returned - only whether each one is set.
    /// </summary>
    IAsyncEnumerable<GitLabDuoWorkflowFlowCallbackHook> ListFlowCallbacksAsync(
        DuoWorkflowFlowCallbackListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Registers an HTTPS endpoint for flow lifecycle callbacks. Pass the returned
    ///     <see cref="GitLabDuoWorkflowFlowCallbackHook.Id" /> as
    ///     <see cref="CreateDuoWorkflowRequest.CallbackHookId" /> to have GitLab push a flow's events
    ///     instead of the client polling for them.
    /// </summary>
    Task<GitLabDuoWorkflowFlowCallbackHook> RegisterFlowCallbackAsync(
        RegisterDuoWorkflowFlowCallbackRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one registered flow callback endpoint.</summary>
    Task<GitLabDuoWorkflowFlowCallbackHook> GetFlowCallbackAsync(long flowCallbackId,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a registered flow callback endpoint so it stops receiving deliveries.</summary>
    Task DeleteFlowCallbackAsync(long flowCallbackId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates and starts a flow. The response carries the new flow's id, which every other method
    ///     here takes; the spec declares no schema for it, so it is returned as raw JSON.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> CreateAsync(CreateDuoWorkflowRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The agent-initiated sibling of <see cref="CreateAsync" />, reachable with an
    ///     <c>ai_workflows</c>-scoped token. It refuses agent-privilege parameters outright, which is why
    ///     it takes a different request type.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> CreateAgentWorkflowAsync(CreateDuoAgentWorkflowRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a flow's details.</summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> GetAsync(long workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies a state transition to a flow - <c>finish</c>, <c>drop</c> and so on. An internal
    ///     operation: this is how the Duo Workflow Service reports a flow's progress back to GitLab.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> UpdateStatusAsync(long workflowId, UpdateDuoWorkflowStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every agent privilege with its id, name, description and whether it is enabled by
    ///     default - the vocabulary behind <see cref="CreateDuoWorkflowRequest.AgentPrivileges" />.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> ListAgentPrivilegesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resumes a flow that paused for human approval.
    ///     <para>
    ///         The flow id is a <see cref="string" /> here, not a number: the spec types this route's
    ///         <c>workflow_id</c> as free text, so it is percent-encoded on the way out - pass it raw.
    ///     </para>
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> ResumeAsync(string workflowId, ResumeDuoWorkflowRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a flow's full trace as JSON Lines - one JSON object per checkpoint event, in
    ///     chronological order. The body is not JSON, so it is streamed rather than deserialized.
    ///     <para>
    ///         The caller owns the returned <see cref="GitLabFileResponse" /> and must
    ///         <c>await using</c> it: it holds the HTTP response and its connection open until disposed.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabForbiddenException">
    ///     <see cref="DuoWorkflowTraceOptions.Full" /> was requested by someone other than the flow's owner.
    /// </exception>
    Task<GitLabFileResponse> GetTraceAsync(long workflowId, DuoWorkflowTraceOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a flow's LangGraph checkpoints. An internal operation - this is the flow service's own
    ///     persistence layer.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> ListCheckpointsAsync(long workflowId, DuoWorkflowCheckpointListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Stores one LangGraph checkpoint for a flow.</summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> CreateCheckpointAsync(long workflowId, CreateDuoWorkflowCheckpointRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one checkpoint by its LangGraph <c>thread_ts</c>, reconstructed from the incremental
    ///     channel blobs. The timestamp is free text and is escaped into the query string, so pass it raw.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> GetCheckpointByThreadTimestampAsync(long workflowId, string threadTimestamp,
        bool? acceptCompressed = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one checkpoint by its numeric id.</summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> GetCheckpointAsync(long workflowId, long checkpointId, bool? acceptCompressed = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records a batch of checkpoint writes against one <c>thread_ts</c>.</summary>
    Task CreateCheckpointWritesAsync(long workflowId, CreateDuoWorkflowCheckpointWritesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the events queued against a flow.</summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> ListEventsAsync(long workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Posts an event to a running flow - pausing or stopping it, or handing the agent a message or an
    ///     answer to a question it asked.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> CreateEventAsync(long workflowId, CreateDuoWorkflowEventRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Moves one flow event between <c>queued</c> and <c>delivered</c>.</summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> UpdateEventAsync(long workflowId, long eventId, UpdateDuoWorkflowEventRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Ingests a batch of AI audit events (CloudEvents v1.0 envelopes) emitted by the Duo Workflow
    ///     Service for one flow.
    /// </summary>
    Task IngestAuditEventsAsync(long workflowId, IngestDuoWorkflowAuditEventsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the connection details - endpoint and a short-lived token - a client needs to talk to the
    ///     Duo Agent Platform Service directly instead of through GitLab.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    /// <exception cref="Exceptions.GitLabRateLimitExceededException">
    ///     The per-user quota for issuing direct-access tokens is exhausted.
    /// </exception>
    Task<JsonElement> RequestDirectAccessAsync(DuoWorkflowDirectAccessRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the Duo Agent Platform tools available, optionally narrowed to one flow definition such
    ///     as <c>software_developer</c>.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> ListToolsAsync(string? workflowDefinition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the WebSocket connection details for streaming a flow. Passing an existing
    ///     <paramref name="workflowId" /> reuses that flow's stored model selection, so a reconnect stays
    ///     on the same provider.
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> GetWebSocketConnectionAsync(string? workflowId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes an <c>ai_workflows</c>-scoped token, ending a flow's direct access.</summary>
    Task RevokeTokenAsync(RevokeDuoWorkflowTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Posts the review comments a Duo Code Review flow produced onto its merge request.</summary>
    Task AddCodeReviewCommentsAsync(AddDuoCodeReviewCommentsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetches the Duo Code Review custom instructions that apply to one merge request.
    ///     <para>
    ///         <paramref name="projectId" /> is a query parameter on this endpoint rather than a path
    ///         segment, so it is a plain string (an id or a namespaced path) and is escaped on the way out.
    ///     </para>
    /// </summary>
    /// <returns>GitLab's raw response body.</returns>
    Task<JsonElement> GetCodeReviewCustomInstructionsAsync(string projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Submits the categorical claims and summary produced by the risk classification tool for a merge
    ///     request. GitLab answers <c>204</c>, so there is nothing to return.
    /// </summary>
    Task SubmitRiskClassificationResultsAsync(SubmitDuoWorkflowRiskClassificationRequest request,
        CancellationToken cancellationToken = default);
}