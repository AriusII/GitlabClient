using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for the instance-wide audit log (<c>GET /audit_events</c>). Administrators only.</summary>
[GitLabQuery]
public readonly record struct AuditEventListOptions
{
    /// <summary>
    ///     Restricts the log to events against this kind of entity, for example "Project" or "User".
    ///     GitLab requires this alongside <see cref="EntityId" /> when either is supplied. Left as free
    ///     text - the spec declares no closed vocabulary for it.
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>Restricts the log to events against this specific entity. Pair with <see cref="EntityType" />.</summary>
    public long? EntityId { get; init; }

    /// <summary>Return only events recorded after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only events recorded before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}