using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a group's milestones (<c>GET /groups/:id/milestones</c>). Separate from
///     <see cref="MilestoneListOptions" /> because only the group route accepts <c>include_descendants</c>;
///     sharing one record would let that filter be sent to the project route, where GitLab answers 200 and
///     silently ignores it.
/// </summary>
[GitLabQuery]
public readonly record struct GroupMilestoneListOptions
{
    /// <summary>Filters the milestones by their lifecycle state.</summary>
    public GitLabMilestoneStateFilter? State { get; init; }

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

    /// <summary>Include milestones from every subgroup and subproject. Group route only.</summary>
    public bool? IncludeDescendants { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public int? PerPage { get; init; }
}