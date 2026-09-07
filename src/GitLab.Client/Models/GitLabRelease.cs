using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>A release in a project's repository, as returned by the GitLab Releases API.</summary>
public sealed record GitLabRelease
{
    public required string TagName { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    /// <summary>The description rendered to HTML, present only when the caller asked for it.</summary>
    public string? DescriptionHtml { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ReleasedAt { get; init; }

    /// <summary>Whether <see cref="ReleasedAt" /> is still in the future.</summary>
    public bool? UpcomingRelease { get; init; }

    public GitLabUser? Author { get; init; }

    /// <summary>The commit the release's tag points at.</summary>
    public GitLabCommit? Commit { get; init; }

    /// <summary>The milestones associated with the release, if any.</summary>
    public IReadOnlyList<GitLabMilestone>? Milestones { get; init; }

    /// <summary>The instance-relative path of <see cref="Commit" />, for example <c>/group/project/-/commit/abc</c>.</summary>
    public string? CommitPath { get; init; }

    /// <summary>The instance-relative path of the release's tag.</summary>
    public string? TagPath { get; init; }

    /// <summary>The source archives and asset links attached to the release.</summary>
    public GitLabReleaseAssets? Assets { get; init; }

    /// <summary>The evidence snapshots collected for the release.</summary>
    public IReadOnlyList<GitLabReleaseEvidence>? Evidences { get; init; }

    [JsonPropertyName("_links")] public GitLabReleaseLinks? Links { get; init; }

    /// <summary>
    ///     GitLab nests the release's URL under <c>_links.self</c> rather than a top-level field; this is a convenience
    ///     accessor over <see cref="Links" />, not a directly deserialized property.
    /// </summary>
    public Uri? WebUrl => Links?.Self;
}