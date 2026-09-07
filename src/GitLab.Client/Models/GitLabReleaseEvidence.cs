namespace GitLab.Client.Models;

/// <summary>
///     One evidence snapshot collected for a release - the JSON record GitLab takes of the release, its
///     milestones and their issues at the moment it was created.
/// </summary>
public sealed record GitLabReleaseEvidence
{
    /// <summary>The SHA-256 of the collected evidence document.</summary>
    public string? Sha { get; init; }

    /// <summary>
    ///     Where the evidence document can be fetched. Named <c>filepath</c> on the wire, but GitLab sends an
    ///     absolute URL.
    /// </summary>
    public Uri? Filepath { get; init; }

    public DateTimeOffset? CollectedAt { get; init; }
}