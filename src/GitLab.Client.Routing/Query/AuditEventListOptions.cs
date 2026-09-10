using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for the instance-wide audit log (<c>GET /audit_events</c>). Administrators only.</summary>
[GitLabQuery]
public readonly record struct AuditEventListOptions
{
    /// <summary>
    ///     Restricts the log to events against this kind of entity. GitLab requires this parameter for every
    ///     instance-audit-log request and constrains it to this closed vocabulary.
    /// </summary>
    public required GitLabAuditEventEntityType? EntityType { get; init; }

    /// <summary>Restricts the log to events against this specific entity.</summary>
    public long? EntityId { get; init; }

    /// <summary>Return only events recorded after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only events recorded before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}