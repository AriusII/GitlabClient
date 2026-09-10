using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A group epic returned by GitLab's legacy Epics REST API. GitLab 19.x keeps this API for
///     compatibility even though new instances model epics as work items.
/// </summary>
public sealed record GitLabEpic
{
    public long? Id { get; init; }

    public long? WorkItemId { get; init; }

    public long? Iid { get; init; }

    public string? Color { get; init; }

    public string? TextColor { get; init; }

    public long? GroupId { get; init; }

    public long? ParentId { get; init; }

    public long? ParentIid { get; init; }

    public bool? Imported { get; init; }

    public string? ImportedFrom { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public bool? Confidential { get; init; }

    public GitLabBasicUser? Author { get; init; }

    public DateTimeOffset? StartDate { get; init; }

    public bool? StartDateIsFixed { get; init; }

    public DateTimeOffset? StartDateFixed { get; init; }

    public DateTimeOffset? StartDateFromInheritedSource { get; init; }

    public DateTimeOffset? StartDateFromMilestones { get; init; }

    public DateTimeOffset? EndDate { get; init; }

    public DateTimeOffset? DueDate { get; init; }

    public bool? DueDateIsFixed { get; init; }

    public DateTimeOffset? DueDateFixed { get; init; }

    public DateTimeOffset? DueDateFromInheritedSource { get; init; }

    public DateTimeOffset? DueDateFromMilestones { get; init; }

    public string? State { get; init; }

    public Uri? WebEditUrl { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>
    ///     The short, relative, and fully qualified forms of this epic's reference. The representation is
    ///     the same <c>APIEntitiesIssuableReferences</c> object GitLab uses for issues and merge requests.
    /// </summary>
    public GitLabIssuableReferences? References { get; init; }

    public string? Reference { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    public int? Upvotes { get; init; }

    public int? Downvotes { get; init; }

    public bool? Subscribed { get; init; }

    /// <summary>The route links GitLab returns for this epic; their key set is intentionally open-ended.</summary>
    [JsonPropertyName("_links")]
    public JsonElement? Links { get; init; }
}