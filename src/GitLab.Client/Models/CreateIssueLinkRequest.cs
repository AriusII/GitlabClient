namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/issues/:issue_iid/links</c>.</summary>
public sealed record CreateIssueLinkRequest
{
    /// <summary>
    ///     The target project, as its numeric id or its URL-encoded path. Typed as a string because GitLab
    ///     accepts either, and because this value travels in a JSON body rather than in the route - the
    ///     <c>ProjectId</c> value object encodes itself for routes and is not interchangeable here.
    /// </summary>
    public required string TargetProjectId { get; init; }

    /// <summary>The IID of the issue to link to, within the target project.</summary>
    public required long TargetIssueIid { get; init; }

    /// <summary>Defaults to <see cref="GitLabIssueLinkType.RelatesTo" /> when left null.</summary>
    public GitLabIssueLinkType? LinkType { get; init; }
}