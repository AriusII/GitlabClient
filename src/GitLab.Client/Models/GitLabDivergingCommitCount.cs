namespace GitLab.Client.Models;

/// <summary>
///     How far two refs have diverged (<c>GET /projects/:id/repository/diverging_commits</c>):
///     the number of commits the "to" ref is behind and ahead of the "from" ref.
/// </summary>
public sealed record GitLabDivergingCommitCount
{
    public required int Behind { get; init; }

    public required int Ahead { get; init; }
}