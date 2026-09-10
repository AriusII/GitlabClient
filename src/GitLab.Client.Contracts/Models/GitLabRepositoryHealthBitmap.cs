namespace GitLab.Client.Models;

/// <summary>
///     The state of a reachability bitmap, from <c>GET /projects/:id/repository/health</c>. Used for both
///     the packfile bitmap and the multi-pack-index bitmap.
/// </summary>
public sealed record GitLabRepositoryHealthBitmap
{
    public bool? HasHashCache { get; init; }

    public bool? HasLookupTable { get; init; }

    public long? Version { get; init; }
}