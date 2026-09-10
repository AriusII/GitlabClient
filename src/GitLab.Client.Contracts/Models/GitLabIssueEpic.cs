namespace GitLab.Client.Models;

/// <summary>
///     The compact epic projection embedded in GitLab issue payloads. This is intentionally separate from
///     a full Epic resource: the Issues API only returns these six fields, and treating it as a complete
///     epic would promise data that the endpoint does not supply.
/// </summary>
public sealed record GitLabIssueEpic
{
    /// <summary>The epic database ID, represented as a string by the GitLab response schema.</summary>
    public string? Id { get; init; }

    /// <summary>The epic's group-scoped internal ID.</summary>
    public string? Iid { get; init; }

    public string? Title { get; init; }

    /// <summary>The web URL of the epic, when the caller is allowed to view it.</summary>
    public Uri? Url { get; init; }

    /// <summary>The owning group's database ID, represented as a string by the response schema.</summary>
    public string? GroupId { get; init; }

    /// <summary>The human-readable epic end date supplied by GitLab.</summary>
    public string? HumanReadableEndDate { get; init; }

    /// <summary>The human-readable epic timestamp supplied by GitLab.</summary>
    public string? HumanReadableTimestamp { get; init; }
}