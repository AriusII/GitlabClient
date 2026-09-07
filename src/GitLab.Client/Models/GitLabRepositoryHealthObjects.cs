namespace GitLab.Client.Models;

/// <summary>
///     How a repository stores its objects, from <c>GET /projects/:id/repository/health</c>. Large
///     <see cref="LooseObjectsCount" /> or <see cref="CruftCount" /> values are the usual sign that a
///     repository needs housekeeping.
/// </summary>
public sealed record GitLabRepositoryHealthObjects
{
    /// <summary>Total size in bytes of all packed objects.</summary>
    public long? Size { get; init; }

    /// <summary>Size in bytes of objects still reachable and recently written.</summary>
    public long? RecentSize { get; init; }

    /// <summary>Size in bytes of objects that are no longer recent.</summary>
    public long? StaleSize { get; init; }

    /// <summary>Size in bytes of packfiles marked with a <c>.keep</c> file.</summary>
    public long? KeepSize { get; init; }

    public long? PackfileCount { get; init; }

    public long? ReverseIndexCount { get; init; }

    /// <summary>Number of cruft packs - packs holding unreachable objects awaiting expiry.</summary>
    public long? CruftCount { get; init; }

    public long? KeepCount { get; init; }

    public long? LooseObjectsCount { get; init; }

    public long? StaleLooseObjectsCount { get; init; }

    public long? LooseObjectsGarbageCount { get; init; }
}