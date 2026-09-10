using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of an instance-wide or group audit log - <c>GET /audit_events</c>,
///     <c>GET /audit_events/:id</c> and <c>GET /groups/:id/audit_events</c>.
///     <para>
///         Shares its shape with <see cref="GitLabProjectAuditEvent" /> (GitLab's audit event entity is
///         the same across scopes) but stays a distinct type: the project, group and instance audit
///         logs are three separate route families with independent access checks, and collapsing them
///         into one model would blur which endpoint a given instance came from.
///     </para>
/// </summary>
public sealed record GitLabAuditEvent
{
    public required long Id { get; init; }

    /// <summary>The user who performed the audited action.</summary>
    public long? AuthorId { get; init; }

    /// <summary>The audited entity's ID - a group ID, a user ID, and so on, depending on <see cref="EntityType" />.</summary>
    public long? EntityId { get; init; }

    /// <summary>The kind of entity <see cref="EntityId" /> refers to, for example "Group" or "User".</summary>
    public string? EntityType { get; init; }

    /// <summary>GitLab's name for the audited action, for example "group_deploy_token_create".</summary>
    public string? EventName { get; init; }

    /// <summary>
    ///     The action's payload, kept as raw JSON. Its shape depends entirely on <see cref="EventName" /> -
    ///     the spec types it as an untyped object - so it is surfaced verbatim rather than guessed at. It
    ///     may itself carry sensitive detail (target usernames, IP addresses, changed permissions);
    ///     callers should treat it with the same care as the rest of an audit record.
    /// </summary>
    public JsonElement? Details { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}