using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of a project's audit log - <c>GET /projects/:id/audit_events</c> and
///     <c>GET /projects/:id/audit_events/:audit_event_id</c>.
/// </summary>
public sealed record GitLabProjectAuditEvent
{
    public required long Id { get; init; }

    /// <summary>The user who performed the audited action.</summary>
    public long? AuthorId { get; init; }

    /// <summary>The audited entity - the project's ID, for a project audit event.</summary>
    public long? EntityId { get; init; }

    /// <summary>The kind of entity <see cref="EntityId" /> refers to, for example "Project".</summary>
    public string? EntityType { get; init; }

    /// <summary>GitLab's name for the audited action, for example "project_archived".</summary>
    public string? EventName { get; init; }

    /// <summary>
    ///     The action's payload, kept as raw JSON. Its shape depends entirely on <see cref="EventName" /> -
    ///     the spec types it as an untyped object - so it is surfaced verbatim rather than guessed at.
    /// </summary>
    public JsonElement? Details { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}