using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ai_agent/audit_events</c>.</summary>
public sealed record IngestAgentAuditEventsRequest
{
    /// <summary>The <see cref="GitLabAgentSession.Id" /> the events belong to.</summary>
    public required long SessionId { get; init; }

    /// <summary>
    ///     The batch to ingest, at most 500 events. GitLab types each event as an untyped object and
    ///     deduplicates the batch by the event's own <c>cloud_event_id</c>, so the events are carried as raw
    ///     <see cref="JsonElement" /> values rather than forced into a shape the spec does not promise.
    /// </summary>
    public required IReadOnlyList<JsonElement> Events { get; init; }
}