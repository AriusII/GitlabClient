using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for <c>GET /groups/:id/issues_statistics</c>. The counts are computed over exactly the
///     issues these filters select, so an unfiltered call counts every issue in the group's hierarchy.
/// </summary>
[GitLabQuery]
public sealed record GroupIssueStatisticsOptions
{
    /// <summary>Keeps only issues carrying every one of these labels.</summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>
    ///     Excludes issues carrying any of these labels. GitLab spells its negated filters <c>not[...]</c>, which no
    ///     naming convention derives, so the wire name is explicit.
    /// </summary>
    [QueryParameter("not[labels]")]
    public IReadOnlyList<string>? NotLabels { get; init; }

    public string? Milestone { get; init; }

    [QueryParameter("not[milestone]")] public string? NotMilestone { get; init; }

    /// <summary>One of <c>Any</c>, <c>None</c>, <c>Upcoming</c> or <c>Started</c>.</summary>
    public string? MilestoneId { get; init; }

    /// <summary>Keeps only issues with these project-scoped IIDs.</summary>
    public IReadOnlyList<long>? Iids { get; init; }

    [QueryParameter("not[iids]")] public IReadOnlyList<long>? NotIids { get; init; }

    /// <summary>Free-text search over the fields named by <see cref="SearchIn" />.</summary>
    public string? Search { get; init; }

    /// <summary>Which fields <see cref="Search" /> covers: <c>title</c>, <c>description</c>, or both comma-separated.</summary>
    [QueryParameter("in")]
    public string? SearchIn { get; init; }

    public long? AuthorId { get; init; }

    [QueryParameter("not[author_id]")] public long? NotAuthorId { get; init; }

    public string? AuthorUsername { get; init; }

    public long? AssigneeId { get; init; }

    [QueryParameter("not[assignee_id]")] public long? NotAssigneeId { get; init; }

    public IReadOnlyList<string>? AssigneeUsername { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <summary>One of <c>created_by_me</c>, <c>assigned_to_me</c> or <c>all</c>.</summary>
    public string? Scope { get; init; }

    /// <summary>Keeps only issues the caller reacted to with this emoji; <c>None</c> and <c>Any</c> also work.</summary>
    public string? MyReactionEmoji { get; init; }

    public bool? Confidential { get; init; }

    public int? Weight { get; init; }

    public long? EpicId { get; init; }

    /// <summary>One of <c>on_track</c>, <c>needs_attention</c>, <c>at_risk</c>, <c>none</c> or <c>any</c>.</summary>
    public string? HealthStatus { get; init; }

    public long? IterationId { get; init; }

    public string? IterationTitle { get; init; }
}