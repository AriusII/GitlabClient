namespace GitLab.Client.Models.Requests;

/// <summary>Request body for creating a two-way link between epics.</summary>
public sealed record CreateRelatedEpicLinkRequest
{
    /// <summary>
    ///     The target group id or URL-encoded path. This stays a string because the GitLab 19.4 wire
    ///     schema declares only a string, unlike route parameters which use <c>GroupId</c>.
    /// </summary>
    public required string TargetGroupId { get; init; }

    public required long TargetEpicIid { get; init; }

    public GitLabEpicLinkType? LinkType { get; init; }
}