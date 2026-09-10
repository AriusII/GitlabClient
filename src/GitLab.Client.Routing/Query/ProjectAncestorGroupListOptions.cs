using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for a project's ancestor groups (<c>GET /projects/:id/groups</c>).</summary>
[GitLabQuery]
public readonly record struct ProjectAncestorGroupListOptions
{
    /// <summary>Return only groups matching this search term.</summary>
    public string? Search { get; init; }

    /// <summary>Leave these group IDs out of the result.</summary>
    public IReadOnlyList<long>? SkipGroups { get; init; }

    /// <summary>Also include the groups the project is shared with, not only its ancestors.</summary>
    public bool? WithShared { get; init; }

    /// <summary>Restrict the shared groups to those the authenticated user can see.</summary>
    public bool? SharedVisibleOnly { get; init; }

    /// <summary>Restrict the shared groups to those where the caller has at least this access level.</summary>
    public int? SharedMinAccessLevel { get; init; }

    public int? PerPage { get; init; }
}