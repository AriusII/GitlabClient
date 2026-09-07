using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the issue statistics endpoints (<c>GET /issues_statistics</c>,
///     <c>GET /groups/:id/issues_statistics</c>, <c>GET /projects/:id/issues_statistics</c>).
///     <para>
///         Deliberately a separate type from <see cref="IssueListOptions" />: the statistics endpoints count
///         issues in every state, so they take neither <c>state</c> nor the ordering and pagination
///         parameters, and offering those here would invite calls whose filters GitLab silently ignores.
///     </para>
/// </summary>
[GitLabQuery]
public sealed record IssueStatisticsOptions
{
    /// <inheritdoc cref="IssueListOptions.Labels" />
    public IReadOnlyList<string>? Labels { get; init; }

    /// <inheritdoc cref="IssueListOptions.Milestone" />
    public string? Milestone { get; init; }

    /// <inheritdoc cref="IssueListOptions.MilestoneId" />
    public GitLabIssueMilestoneFilter? MilestoneId { get; init; }

    public IReadOnlyList<long>? Iids { get; init; }

    public string? Search { get; init; }

    /// <inheritdoc cref="IssueListOptions.In" />
    public string? In { get; init; }

    /// <inheritdoc cref="IssueListOptions.AuthorId" />
    public long? AuthorId { get; init; }

    /// <inheritdoc cref="IssueListOptions.AuthorUsername" />
    public string? AuthorUsername { get; init; }

    /// <inheritdoc cref="IssueListOptions.AssigneeId" />
    public string? AssigneeId { get; init; }

    public IReadOnlyList<string>? AssigneeUsername { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <inheritdoc cref="IssueListOptions.NotLabels" />
    [QueryParameter("not[labels]")]
    public IReadOnlyList<string>? NotLabels { get; init; }

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

    /// <inheritdoc cref="IssueListOptions.MyReactionEmoji" />
    public string? MyReactionEmoji { get; init; }

    public bool? Confidential { get; init; }

    /// <inheritdoc cref="IssueListOptions.Weight" />
    public string? Weight { get; init; }

    public GitLabIssueHealthStatus? HealthStatus { get; init; }

    /// <inheritdoc cref="IssueListOptions.IterationId" />
    public string? IterationId { get; init; }

    public string? IterationTitle { get; init; }
}