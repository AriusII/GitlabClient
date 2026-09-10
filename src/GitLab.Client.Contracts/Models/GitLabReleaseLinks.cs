namespace GitLab.Client.Models;

/// <summary>The <c>_links</c> object GitLab nests under a release.</summary>
public sealed record GitLabReleaseLinks
{
    public Uri? Self { get; init; }

    /// <summary>The release's edit page. Present only for a caller who may edit it.</summary>
    public Uri? EditUrl { get; init; }

    public Uri? ClosedIssuesUrl { get; init; }

    public Uri? ClosedMergeRequestsUrl { get; init; }

    public Uri? MergedMergeRequestsUrl { get; init; }

    public Uri? OpenedIssuesUrl { get; init; }

    public Uri? OpenedMergeRequestsUrl { get; init; }
}