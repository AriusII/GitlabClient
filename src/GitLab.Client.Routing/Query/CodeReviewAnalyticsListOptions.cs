using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for <c>GET /analytics/code_review</c>. The endpoint's required <c>project_id</c> is an
///     ordinary method parameter rather than a member here, since <c>[GitLabQuery]</c> properties must
///     all be nullable (GLQ0002) and it is never omitted.
/// </summary>
[GitLabQuery]
public readonly record struct CodeReviewAnalyticsListOptions
{
    /// <summary>Only merge requests carrying every one of these labels.</summary>
    public IReadOnlyList<string>? LabelName { get; init; }

    /// <summary>Only merge requests attached to the milestone with this title.</summary>
    public string? MilestoneTitle { get; init; }

    /// <summary>
    ///     Excludes merge requests carrying any of these labels. GitLab spells the negated filter
    ///     <c>not[label_name]</c>, which no naming convention derives, so the wire name is explicit.
    /// </summary>
    [QueryParameter("not[label_name]")]
    public IReadOnlyList<string>? NotLabelName { get; init; }

    /// <summary>Excludes merge requests attached to the milestone with this title.</summary>
    [QueryParameter("not[milestone_title]")]
    public string? NotMilestoneTitle { get; init; }

    public int? Page { get; init; }

    public int? PerPage { get; init; }
}