using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One CloudEvents v1.0 envelope in a batch posted to
///     <c>POST /ai/duo_workflows/workflows/:id/audit_events</c>.
/// </summary>
public sealed record DuoWorkflowAuditEvent
{
    /// <summary>The CloudEvent id, a UUID. GitLab deduplicates a batch by this value.</summary>
    public required string Id { get; init; }

    /// <summary>The event type - for example <c>ai_llm_input_sent</c>.</summary>
    public required string Type { get; init; }

    /// <summary>The CloudEvent source.</summary>
    public required string Source { get; init; }

    /// <summary>The gateway's timestamp for the event.</summary>
    public required DateTimeOffset Time { get; init; }

    /// <summary>
    ///     The event-specific payload. The spec types it as an untyped object, so it is carried as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Data { get; init; }
}