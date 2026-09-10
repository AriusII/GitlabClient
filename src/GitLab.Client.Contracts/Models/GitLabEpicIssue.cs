using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     An issue as projected by an epic. This remains distinct from <see cref="GitLabIssue" /> because the
///     Epics schema additionally identifies the epic-issue association and its relative position.
/// </summary>
public sealed record GitLabEpicIssue
{
    public long? Id { get; init; }

    public long? Iid { get; init; }

    public long? ProjectId { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public GitLabBasicUser? ClosedBy { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    public GitLabMilestone? Milestone { get; init; }

    public IReadOnlyList<GitLabBasicUser>? Assignees { get; init; }

    public GitLabBasicUser? Author { get; init; }

    public string? Type { get; init; }

    public GitLabBasicUser? Assignee { get; init; }

    public int? UserNotesCount { get; init; }

    public int? MergeRequestsCount { get; init; }

    public int? Upvotes { get; init; }

    public int? Downvotes { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? DueDate { get; init; }

    public bool? Confidential { get; init; }

    public bool? DiscussionLocked { get; init; }

    public string? IssueType { get; init; }

    public Uri? WebUrl { get; init; }

    public GitLabTimeStats? TimeStats { get; init; }

    public GitLabTaskCompletionStatus? TaskCompletionStatus { get; init; }

    public int? Weight { get; init; }

    public int? BlockingIssuesCount { get; init; }

    public bool? HasTasks { get; init; }

    public string? TaskStatus { get; init; }

    [JsonPropertyName("_links")] public JsonElement? Links { get; init; }

    public GitLabIssuableReferences? References { get; init; }

    public string? Severity { get; init; }

    public bool? Subscribed { get; init; }

    public long? MovedToId { get; init; }

    public bool? Imported { get; init; }

    public string? ImportedFrom { get; init; }

    public string? ServiceDeskReplyTo { get; init; }

    public string? EpicIid { get; init; }

    public GitLabIssueEpic? Epic { get; init; }

    public GitLabIteration? Iteration { get; init; }

    public string? HealthStatus { get; init; }

    /// <summary>The id of the epic-issue association, rather than the issue id.</summary>
    public long? EpicIssueId { get; init; }

    public int? RelativePosition { get; init; }
}