using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for a group's audit log (<c>GET /groups/:id/audit_events</c>).</summary>
[GitLabQuery]
public readonly record struct GroupAuditEventListOptions
{
    /// <summary>Return only events recorded after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only events recorded before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}