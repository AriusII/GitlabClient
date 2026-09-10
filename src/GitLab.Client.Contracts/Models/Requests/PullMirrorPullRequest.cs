namespace GitLab.Client.Models.Requests;

/// <summary>The GitHub pull request being relayed through <see cref="TriggerPullMirrorRequest" />.</summary>
public sealed record PullMirrorPullRequest
{
    public long? Number { get; init; }

    public PullMirrorPullRequestRef? Head { get; init; }

    public PullMirrorPullRequestRef? Base { get; init; }
}