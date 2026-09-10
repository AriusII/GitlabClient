using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class DuoWorkflowsClient(IGitLabApiConnection connection) : IDuoWorkflowsClient
{
    public IAsyncEnumerable<GitLabDuoWorkflowFlowCallbackHook> ListFlowCallbacksAsync(
        DuoWorkflowFlowCallbackListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            Route().Literal("flow_callbacks").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDuoWorkflowFlowCallbackHookArray,
            cancellationToken);
    }

    public Task<GitLabDuoWorkflowFlowCallbackHook> RegisterFlowCallbackAsync(
        RegisterDuoWorkflowFlowCallbackRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("flow_callbacks").Build(),
            request,
            GitLabJsonContext.Default.RegisterDuoWorkflowFlowCallbackRequest,
            GitLabJsonContext.Default.GitLabDuoWorkflowFlowCallbackHook,
            cancellationToken);
    }

    public Task<GitLabDuoWorkflowFlowCallbackHook> GetFlowCallbackAsync(long flowCallbackId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Route().Literal("flow_callbacks").Segment(flowCallbackId).Build(),
            GitLabJsonContext.Default.GitLabDuoWorkflowFlowCallbackHook,
            cancellationToken);
    }

    public Task DeleteFlowCallbackAsync(long flowCallbackId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            Route().Literal("flow_callbacks").Segment(flowCallbackId).Build(),
            cancellationToken);
    }

    public Task<JsonElement> CreateAsync(CreateDuoWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("workflows").Build(),
            request,
            GitLabJsonContext.Default.CreateDuoWorkflowRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> CreateAgentWorkflowAsync(CreateDuoAgentWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("agent_workflows").Build(),
            request,
            GitLabJsonContext.Default.CreateDuoAgentWorkflowRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetAsync(long workflowId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            WorkflowRoute(workflowId).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> UpdateStatusAsync(long workflowId, UpdateDuoWorkflowStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            WorkflowRoute(workflowId).Build(),
            request,
            GitLabJsonContext.Default.UpdateDuoWorkflowStatusRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> ListAgentPrivilegesAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Route().Literal("workflows").Literal("agent_privileges").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> ResumeAsync(string workflowId, ResumeDuoWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        // The spec types this route's {workflow_id} as a string, not an integer, so it is escaped rather
        // than emitted as a numeric segment.
        return connection.PostAsync(
            Route().Literal("workflows").Escaped(workflowId).Literal("resume").Build(),
            request,
            GitLabJsonContext.Default.ResumeDuoWorkflowRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetTraceAsync(long workflowId, DuoWorkflowTraceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            WorkflowRoute(workflowId).Literal("trace.jsonl").QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<JsonElement> ListCheckpointsAsync(long workflowId,
        DuoWorkflowCheckpointListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            WorkflowRoute(workflowId).Literal("checkpoints").QueryFrom(options).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> CreateCheckpointAsync(long workflowId, CreateDuoWorkflowCheckpointRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            WorkflowRoute(workflowId).Literal("checkpoints").Build(),
            request,
            GitLabJsonContext.Default.CreateDuoWorkflowCheckpointRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetCheckpointByThreadTimestampAsync(long workflowId, string threadTimestamp,
        bool? acceptCompressed = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            WorkflowRoute(workflowId).Literal("checkpoints").Literal("by_thread_ts")
                .Query("thread_ts", threadTimestamp)
                .Query("accept_compressed", acceptCompressed)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetCheckpointAsync(long workflowId, long checkpointId,
        bool? acceptCompressed = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            WorkflowRoute(workflowId).Literal("checkpoints").Segment(checkpointId)
                .Query("accept_compressed", acceptCompressed)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task CreateCheckpointWritesAsync(long workflowId, CreateDuoWorkflowCheckpointWritesRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            WorkflowRoute(workflowId).Literal("checkpoint_writes_batch").Build(),
            request,
            GitLabJsonContext.Default.CreateDuoWorkflowCheckpointWritesRequest,
            cancellationToken);
    }

    public Task<JsonElement> ListEventsAsync(long workflowId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            WorkflowRoute(workflowId).Literal("events").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> CreateEventAsync(long workflowId, CreateDuoWorkflowEventRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            WorkflowRoute(workflowId).Literal("events").Build(),
            request,
            GitLabJsonContext.Default.CreateDuoWorkflowEventRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> UpdateEventAsync(long workflowId, long eventId,
        UpdateDuoWorkflowEventRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            WorkflowRoute(workflowId).Literal("events").Segment(eventId).Build(),
            request,
            GitLabJsonContext.Default.UpdateDuoWorkflowEventRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task IngestAuditEventsAsync(long workflowId, IngestDuoWorkflowAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            WorkflowRoute(workflowId).Literal("audit_events").Build(),
            request,
            GitLabJsonContext.Default.IngestDuoWorkflowAuditEventsRequest,
            cancellationToken);
    }

    public Task<JsonElement> RequestDirectAccessAsync(DuoWorkflowDirectAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("direct_access").Build(),
            request,
            GitLabJsonContext.Default.DuoWorkflowDirectAccessRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> ListToolsAsync(string? workflowDefinition = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Route().Literal("list_tools").Query("workflow_definition", workflowDefinition).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetWebSocketConnectionAsync(string? workflowId = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Route().Literal("ws").Query("workflow_id", workflowId).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task RevokeTokenAsync(RevokeDuoWorkflowTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("revoke_token").Build(),
            request,
            GitLabJsonContext.Default.RevokeDuoWorkflowTokenRequest,
            cancellationToken);
    }

    public Task AddCodeReviewCommentsAsync(AddDuoCodeReviewCommentsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("code_review").Literal("add_comments").Build(),
            request,
            GitLabJsonContext.Default.AddDuoCodeReviewCommentsRequest,
            cancellationToken);
    }

    public Task<JsonElement> GetCodeReviewCustomInstructionsAsync(string projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        // project_id is a query parameter here rather than a path segment, so it is passed as free text
        // and escaped by the route builder - a ProjectId would arrive already encoded and double-encode.
        return connection.GetAsync(
            Route().Literal("code_review").Literal("custom_instructions")
                .Query("project_id", projectId)
                .Query("merge_request_iid", mergeRequestIid)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task SubmitRiskClassificationResultsAsync(SubmitDuoWorkflowRiskClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            Route().Literal("tools").Literal("risk_classification").Literal("results").Build(),
            request,
            GitLabJsonContext.Default.SubmitDuoWorkflowRiskClassificationRequest,
            cancellationToken);
    }

    private static GitLabRouteBuilder Route()
    {
        return GitLabRouteBuilder.Create("ai").Literal("duo_workflows");
    }

    private static GitLabRouteBuilder WorkflowRoute(long workflowId)
    {
        return Route().Literal("workflows").Segment(workflowId);
    }
}