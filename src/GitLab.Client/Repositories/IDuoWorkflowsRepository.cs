using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the GitLab Duo workflows resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDuoWorkflowsService), typeof(IDuoWorkflowsClient))]
internal interface IDuoWorkflowsRepository
{
    IAsyncEnumerable<GitLabDuoWorkflowFlowCallbackHook> ListFlowCallbacksAsync(
        DuoWorkflowFlowCallbackListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDuoWorkflowFlowCallbackHook> RegisterFlowCallbackAsync(
        RegisterDuoWorkflowFlowCallbackRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDuoWorkflowFlowCallbackHook> GetFlowCallbackAsync(long flowCallbackId,
        CancellationToken cancellationToken = default);

    Task DeleteFlowCallbackAsync(long flowCallbackId, CancellationToken cancellationToken = default);

    Task<JsonElement> CreateAsync(CreateDuoWorkflowRequest request, CancellationToken cancellationToken = default);

    Task<JsonElement> CreateAgentWorkflowAsync(CreateDuoAgentWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetAsync(long workflowId, CancellationToken cancellationToken = default);

    Task<JsonElement> UpdateStatusAsync(long workflowId, UpdateDuoWorkflowStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListAgentPrivilegesAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> ResumeAsync(string workflowId, ResumeDuoWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetTraceAsync(long workflowId, DuoWorkflowTraceOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListCheckpointsAsync(long workflowId, DuoWorkflowCheckpointListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> CreateCheckpointAsync(long workflowId, CreateDuoWorkflowCheckpointRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCheckpointByThreadTimestampAsync(long workflowId, string threadTimestamp,
        bool? acceptCompressed = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetCheckpointAsync(long workflowId, long checkpointId, bool? acceptCompressed = null,
        CancellationToken cancellationToken = default);

    Task CreateCheckpointWritesAsync(long workflowId, CreateDuoWorkflowCheckpointWritesRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListEventsAsync(long workflowId, CancellationToken cancellationToken = default);

    Task<JsonElement> CreateEventAsync(long workflowId, CreateDuoWorkflowEventRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> UpdateEventAsync(long workflowId, long eventId, UpdateDuoWorkflowEventRequest request,
        CancellationToken cancellationToken = default);

    Task IngestAuditEventsAsync(long workflowId, IngestDuoWorkflowAuditEventsRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> RequestDirectAccessAsync(DuoWorkflowDirectAccessRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListToolsAsync(string? workflowDefinition = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetWebSocketConnectionAsync(string? workflowId = null,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(RevokeDuoWorkflowTokenRequest request, CancellationToken cancellationToken = default);

    Task AddCodeReviewCommentsAsync(AddDuoCodeReviewCommentsRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCodeReviewCustomInstructionsAsync(string projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task SubmitRiskClassificationResultsAsync(SubmitDuoWorkflowRiskClassificationRequest request,
        CancellationToken cancellationToken = default);
}