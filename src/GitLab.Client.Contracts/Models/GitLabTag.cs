namespace GitLab.Client.Models;

/// <summary>A repository tag, as returned by the GitLab Tags API (<c>/projects/:id/repository/tags</c>).</summary>
public sealed record GitLabTag
{
    public required string Name { get; init; }

    public string? Message { get; init; }

    public required string Target { get; init; }

    public GitLabCommit? Commit { get; init; }

    /// <summary>The release attached to the tag, when one exists.</summary>
    public GitLabTagRelease? Release { get; init; }

    public bool? Protected { get; init; }

    /// <summary>The time at which GitLab created the tag.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}