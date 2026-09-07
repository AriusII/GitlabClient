using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's milestones (<c>GET /projects/:id/milestones</c>).</summary>
[GitLabQuery]
public sealed record MilestoneListOptions
{
    /// <summary>One of <c>"active"</c>, <c>"closed"</c> or <c>"all"</c>.</summary>
    public string? State { get; init; }

    /// <summary>Return only the milestones with these iids.</summary>
    public IReadOnlyList<long>? Iids { get; init; }

    /// <summary>Exact title match.</summary>
    public string? Title { get; init; }

    /// <summary>Keyword matched against the milestone title and description.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Deprecated by GitLab in favour of <see cref="IncludeAncestors" />, and mutually exclusive with it.
    /// </summary>
    public bool? IncludeParentMilestones { get; init; }

    /// <summary>Include milestones from every parent group.</summary>
    public bool? IncludeAncestors { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public int? PerPage { get; init; }
}