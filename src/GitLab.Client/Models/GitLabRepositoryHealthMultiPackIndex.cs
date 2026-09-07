namespace GitLab.Client.Models;

/// <summary>
///     The state of a repository's multi-pack index, from <c>GET /projects/:id/repository/health</c>.
/// </summary>
public sealed record GitLabRepositoryHealthMultiPackIndex
{
    /// <summary>How many packfiles the index covers.</summary>
    public long? PackfileCount { get; init; }

    public long? Version { get; init; }
}