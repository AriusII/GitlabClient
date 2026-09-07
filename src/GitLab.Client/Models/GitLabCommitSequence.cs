namespace GitLab.Client.Models;

/// <summary>
///     How many commits precede a commit on its own history
///     (<c>GET /projects/:id/repository/commits/:sha/sequence</c>) - the equivalent of
///     <c>git rev-list --count</c>.
/// </summary>
public sealed record GitLabCommitSequence
{
    /// <summary>The number of commits in the sequence, the commit itself included.</summary>
    public required long Count { get; init; }
}