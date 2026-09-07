using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of a group's audit log (<c>GET /groups/:id/audit_events/:audit_event_id</c>). Shaped
///     identically to <see cref="GitLabProjectAuditEvent" /> - GitLab's <c>APIEntitiesAuditEvent</c>
///     schema is the same one entity for every audited scope - kept as its own type here rather than
///     reused because the two resources are owned and evolved independently.
/// </summary>
public sealed record GitLabGroupAuditEvent
{
    public required long Id { get; init; }

    /// <summary>The user who performed the audited action.</summary>
    public long? AuthorId { get; init; }

    /// <summary>The audited entity - the group's ID, for a group audit event.</summary>
    public long? EntityId { get; init; }

    /// <summary>The kind of entity <see cref="EntityId" /> refers to, for example "Group".</summary>
    public string? EntityType { get; init; }

    /// <summary>GitLab's name for the audited action, for example "group_archived".</summary>
    public string? EventName { get; init; }

    /// <summary>
    ///     The action's payload, kept as raw JSON. Its shape depends entirely on <see cref="EventName" /> -
    ///     the spec types it as an untyped object - so it is surfaced verbatim rather than guessed at.
    /// </summary>
    public JsonElement? Details { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}