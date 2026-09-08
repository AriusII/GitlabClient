using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for a project's audit log (<c>GET /projects/:id/audit_events</c>).</summary>
[GitLabQuery]
public readonly record struct ProjectAuditEventListOptions
{
    /// <summary>Return only events recorded after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only events recorded before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}