namespace GitLab.Client.Models;

/// <summary>The repository side of a <see cref="PullMirrorPullRequestRef" />.</summary>
public sealed record PullMirrorPullRequestRepo
{
    public string? FullName { get; init; }
}