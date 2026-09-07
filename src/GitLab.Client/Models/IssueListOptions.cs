using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the issue listings - <c>GET /issues</c> (the authenticated user's issues) and
///     <c>GET /projects/:id/issues</c>, which take the same vocabulary.
///     <para>
///         The first seven properties are declared in their original order on purpose: the generated query
///         string follows declaration order, and appending rather than reshuffling keeps the projection
///         stable for callers that pin it.
///     </para>
/// </summary>
[GitLabQuery]
public sealed record IssueListOptions
{
    public GitLabIssueStateFilter? State { get; init; }

    /// <summary>Only issues carrying every one of these labels. <c>None</c> and <c>Any</c> are also accepted.</summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>
    ///     Excludes issues carrying any of these labels. GitLab spells the negated filters
    ///     <c>not[…]</c>, which no naming convention derives, so the wire name is explicit.
    /// </summary>
    [QueryParameter("not[labels]")]
    public IReadOnlyList<string>? NotLabels { get; init; }

    public IReadOnlyList<long>? Iids { get; init; }

    /// <summary>Mutually exclusive with <see cref="AuthorUsername" />.</summary>
    public long? AuthorId { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Returns the full label objects rather than just their titles.</summary>
    public bool? WithLabelsDetails { get; init; }

    /// <summary>Only issues closed by this user.</summary>
    public long? ClosedById { get; init; }

    public GitLabIssueOrderBy? OrderBy { get; init; }

    public GitLabIssueSort? Sort { get; init; }

    public GitLabIssueDueDateFilter? DueDate { get; init; }

    public GitLabIssueType? IssueType { get; init; }

    /// <summary>A milestone title. Mutually exclusive with <see cref="MilestoneId" />.</summary>
    public string? Milestone { get; init; }

    /// <summary>A milestone timebox. Mutually exclusive with <see cref="Milestone" />.</summary>
    public GitLabIssueMilestoneFilter? MilestoneId { get; init; }

    public string? Search { get; init; }

    /// <summary>Which fields <see cref="Search" /> looks at: <c>title</c>, <c>description</c>, or both joined by a comma.</summary>
    public string? In { get; init; }

    /// <summary>Mutually exclusive with <see cref="AuthorId" />.</summary>
    public string? AuthorUsername { get; init; }

    /// <summary>
    ///     A user id, or the strings <c>None</c> and <c>Any</c>. Typed as a string because GitLab overloads
    ///     the parameter with those two sentinels.
    /// </summary>
    public string? AssigneeId { get; init; }

    public IReadOnlyList<string>? AssigneeUsername { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    [QueryParameter("not[milestone]")] public string? NotMilestone { get; init; }

    [QueryParameter("not[milestone_id]")] public GitLabIssueMilestoneFilter? NotMilestoneId { get; init; }

    [QueryParameter("not[iids]")] public IReadOnlyList<long>? NotIids { get; init; }

    [QueryParameter("not[author_id]")] public long? NotAuthorId { get; init; }

    [QueryParameter("not[author_username]")]
    public string? NotAuthorUsername { get; init; }

    [QueryParameter("not[assignee_id]")] public long? NotAssigneeId { get; init; }

    [QueryParameter("not[assignee_username]")]
    public IReadOnlyList<string>? NotAssigneeUsername { get; init; }

    [QueryParameter("not[weight]")] public int? NotWeight { get; init; }

    [QueryParameter("not[iteration_id]")] public string? NotIterationId { get; init; }

    [QueryParameter("not[iteration_title]")]
    public string? NotIterationTitle { get; init; }

    public GitLabIssueScope? Scope { get; init; }

    /// <summary>Only issues the authenticated user reacted to with this emoji. <c>None</c> and <c>Any</c> are also accepted.</summary>
    public string? MyReactionEmoji { get; init; }

    public bool? Confidential { get; init; }

    /// <summary>
    ///     A weight, or the strings <c>None</c> and <c>Any</c>. Typed as a string for the same reason as
    ///     <see cref="AssigneeId" />.
    /// </summary>
    public string? Weight { get; init; }

    public GitLabIssueHealthStatus? HealthStatus { get; init; }

    /// <summary>An iteration id, or the strings <c>None</c>, <c>Any</c> and <c>Current</c>.</summary>
    public string? IterationId { get; init; }

    public string? IterationTitle { get; init; }

    /// <summary>Excludes issues that belong to archived projects.</summary>
    public bool? NonArchived { get; init; }
}