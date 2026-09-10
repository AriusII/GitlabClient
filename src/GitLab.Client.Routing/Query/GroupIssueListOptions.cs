using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for <c>GET /groups/:id/issues</c>, which spans every project in the group and its
///     subgroups. GitLab declares a wider parameter set here than for a project's own issues, so this is
///     a separate type from <see cref="IssueListOptions" /> rather than an extension of it.
/// </summary>
[GitLabQuery]
public sealed record GroupIssueListOptions
{
    /// <summary>One of <c>opened</c>, <c>closed</c> or <c>all</c>.</summary>
    public string? State { get; init; }

    /// <summary>One of <c>issue</c>, <c>incident</c>, <c>test_case</c>, <c>requirement</c>, <c>task</c> or <c>ticket</c>.</summary>
    public string? IssueType { get; init; }

    /// <summary>Returns each label as an object rather than as a bare name.</summary>
    public bool? WithLabelsDetails { get; init; }

    public long? ClosedById { get; init; }

    /// <summary>
    ///     Field to order by. Left as a string because GitLab keeps adding to the vocabulary and an unknown value is
    ///     answered with a 400, not a silent 200.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Order direction, <c>asc</c> or <c>desc</c>.</summary>
    public string? Sort { get; init; }

    /// <summary>
    ///     A relative window: <c>0</c>, <c>any</c>, <c>today</c>, <c>tomorrow</c>, <c>overdue</c>, <c>week</c>,
    ///     <c>month</c> or <c>next_month_and_previous_two_weeks</c>.
    /// </summary>
    public string? DueDate { get; init; }

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

    /// <summary>
    ///     A user ID, or the <c>None</c> and <c>Any</c> sentinels that GitLab accepts for unassigned and assigned
    ///     issues respectively.
    /// </summary>
    public string? AssigneeId { get; init; }

    [QueryParameter("not[assignee_id]")] public long? NotAssigneeId { get; init; }

    /// <summary>Excludes issues assigned to any of these usernames.</summary>
    [QueryParameter("not[assignee_username]")]
    public IReadOnlyList<string>? NotAssigneeUsername { get; init; }

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

    /// <summary>Excludes issues with this weight.</summary>
    [QueryParameter("not[weight]")]
    public int? NotWeight { get; init; }

    public long? EpicId { get; init; }

    /// <summary>One of <c>on_track</c>, <c>needs_attention</c>, <c>at_risk</c>, <c>none</c> or <c>any</c>.</summary>
    public string? HealthStatus { get; init; }

    public long? IterationId { get; init; }

    /// <summary>Excludes issues assigned to this iteration ID, including GitLab's <c>Any</c> and <c>None</c> sentinels.</summary>
    [QueryParameter("not[iteration_id]")]
    public string? NotIterationId { get; init; }

    public string? IterationTitle { get; init; }

    /// <summary>Excludes issues assigned to an iteration with this title.</summary>
    [QueryParameter("not[iteration_title]")]
    public string? NotIterationTitle { get; init; }

    /// <summary>Excludes issues that live in archived projects.</summary>
    public bool? NonArchived { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}