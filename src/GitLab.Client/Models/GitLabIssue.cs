namespace GitLab.Client.Models;

/// <summary>
///     An issue within a GitLab project, as returned by the Issues API.
///     <para>
///         The type is embedded by several other resources (milestones, resource events, issue links), which
///         each project a reduced shape of it. Only the five members GitLab returns everywhere are
///         <c>required</c>; everything else is nullable so a narrower embedding cannot throw a
///         <see cref="System.Text.Json.JsonException" /> on an unrelated call.
///     </para>
/// </summary>
public sealed record GitLabIssue
{
    public required long Id { get; init; }

    public required long Iid { get; init; }

    public long? ProjectId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public GitLabUser? Author { get; init; }

    /// <summary>Who closed the issue. Null while it is open.</summary>
    public GitLabUser? ClosedBy { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    public GitLabMilestone? Milestone { get; init; }

    public IReadOnlyList<GitLabUser>? Assignees { get; init; }

    /// <summary>
    ///     The first assignee. GitLab keeps it for compatibility with single-assignee clients;
    ///     <see cref="Assignees" /> is the complete list.
    /// </summary>
    public GitLabUser? Assignee { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     The work item type in upper case (<c>ISSUE</c>, <c>INCIDENT</c>, ...). GitLab also sends the
    ///     lower-case <see cref="IssueType" />; the two carry the same information.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     <c>issue</c>, <c>incident</c>, <c>test_case</c>, <c>requirement</c>, <c>task</c> or
    ///     <c>ticket</c>. Typed as a string on purpose - the spec leaves the response field unenumerated, so
    ///     a type GitLab adds later must not break deserialization. <see cref="GitLabIssueType" /> is the
    ///     request-side vocabulary.
    /// </summary>
    public string? IssueType { get; init; }

    public int? UserNotesCount { get; init; }

    public int? MergeRequestsCount { get; init; }

    public int? Upvotes { get; init; }

    public int? Downvotes { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? DueDate { get; init; }

    public bool? Confidential { get; init; }

    public bool? DiscussionLocked { get; init; }

    public GitLabTimeStats? TimeStats { get; init; }

    public GitLabTaskCompletionStatus? TaskCompletionStatus { get; init; }

    public GitLabIssuableReferences? References { get; init; }

    public int? Weight { get; init; }

    public int? BlockingIssuesCount { get; init; }

    public bool? HasTasks { get; init; }

    /// <summary>A human summary of the task list, such as <c>2 of 5 tasks completed</c>.</summary>
    public string? TaskStatus { get; init; }

    /// <summary>
    ///     <c>unknown</c>, <c>low</c>, <c>medium</c>, <c>high</c> or <c>critical</c>, and only meaningful on
    ///     incidents. Typed as a string for the same reason as <see cref="IssueType" />.
    /// </summary>
    public string? Severity { get; init; }

    /// <summary>The id of the issue this one was moved to, once it has been moved. Null otherwise.</summary>
    public long? MovedToId { get; init; }

    public bool? Imported { get; init; }

    public string? ImportedFrom { get; init; }

    /// <summary>The Service Desk address the issue was raised from, when it came in by email.</summary>
    public string? ServiceDeskReplyTo { get; init; }

    public GitLabIteration? Iteration { get; init; }

    /// <summary>
    ///     <c>on_track</c>, <c>needs_attention</c> or <c>at_risk</c>. Typed as a string for the same reason
    ///     as <see cref="IssueType" />; <see cref="GitLabIssueHealthStatus" /> is the filter vocabulary.
    /// </summary>
    public string? HealthStatus { get; init; }

    /// <summary>
    ///     Whether the authenticated user is subscribed to this issue. Only returned by the
    ///     Resource subscriptions endpoints; null on the plain list/get responses.
    /// </summary>
    public bool? Subscribed { get; init; }

    /// <summary>
    ///     The id of the relation this issue was reached through. Only returned by
    ///     <c>GET /projects/:id/issues/:issue_iid/links</c>, which answers with the linked issues rather than
    ///     with <see cref="GitLabIssueLink" /> objects; null everywhere else.
    /// </summary>
    public long? IssueLinkId { get; init; }

    /// <inheritdoc cref="IssueLinkId" />
    public string? LinkType { get; init; }

    /// <inheritdoc cref="IssueLinkId" />
    public DateTimeOffset? LinkCreatedAt { get; init; }

    /// <inheritdoc cref="IssueLinkId" />
    public DateTimeOffset? LinkUpdatedAt { get; init; }
}