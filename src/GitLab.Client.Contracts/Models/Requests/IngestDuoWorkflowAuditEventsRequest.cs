namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /ai/duo_workflows/workflows/:id/audit_events</c>.</summary>
public sealed record IngestDuoWorkflowAuditEventsRequest
{
    /// <summary>The batch of CloudEvents envelopes to ingest.</summary>
    public required IReadOnlyList<DuoWorkflowAuditEvent> Events { get; init; }
}