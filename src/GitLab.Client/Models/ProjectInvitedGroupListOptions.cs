using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for the groups a project is shared with (<c>GET /projects/:id/invited_groups</c>).</summary>
[GitLabQuery]
public readonly record struct ProjectInvitedGroupListOptions
{
    /// <summary>Filter by how the group is related to the project - "direct" and/or "inherited".</summary>
    public IReadOnlyList<string>? Relation { get; init; }

    /// <summary>Return only groups matching this search term.</summary>
    public string? Search { get; init; }

    /// <summary>Return only groups where the caller has at least this access level.</summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>Include each group's custom attributes. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    public int? PerPage { get; init; }
}