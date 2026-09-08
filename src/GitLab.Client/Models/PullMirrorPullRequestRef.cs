namespace GitLab.Client.Models;

/// <summary>
///     One side (<see cref="PullMirrorPullRequest.Head" /> or <see cref="PullMirrorPullRequest.Base" />) of
///     a GitHub pull request relayed through <see cref="TriggerPullMirrorRequest" />.
/// </summary>
public sealed record PullMirrorPullRequestRef
{
    public string? Ref { get; init; }

    public string? Sha { get; init; }

    public PullMirrorPullRequestRepo? Repo { get; init; }
}