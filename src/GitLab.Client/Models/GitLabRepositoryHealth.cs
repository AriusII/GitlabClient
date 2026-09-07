namespace GitLab.Client.Models;

/// <summary>
///     Storage statistics for a project repository (<c>GET /projects/:id/repository/health</c>). Requires
///     push access, and GitLab rate-limits report generation to five requests an hour per project.
/// </summary>
/// <remarks>
///     GitLab also sends <c>alternates</c>, the object-pool paths this repository borrows from. It is
///     deliberately not modelled: the spec types it only as an untyped object, and guessing its shape
///     wrong would turn a healthy response into a <see cref="System.Text.Json.JsonException" />. Unmapped
///     members are skipped, so leaving it out costs nothing.
/// </remarks>
public sealed record GitLabRepositoryHealth
{
    /// <summary>The repository's total on-disk size in bytes.</summary>
    public long? Size { get; init; }

    public GitLabRepositoryHealthReferences? References { get; init; }

    public GitLabRepositoryHealthObjects? Objects { get; init; }

    public GitLabRepositoryHealthCommitGraph? CommitGraph { get; init; }

    /// <summary>The packfile reachability bitmap.</summary>
    public GitLabRepositoryHealthBitmap? Bitmap { get; init; }

    public GitLabRepositoryHealthMultiPackIndex? MultiPackIndex { get; init; }

    /// <summary>The multi-pack-index reachability bitmap.</summary>
    public GitLabRepositoryHealthBitmap? MultiPackIndexBitmap { get; init; }

    /// <summary>True when the repository is itself an object pool other repositories borrow from.</summary>
    public bool? IsObjectPool { get; init; }

    public GitLabRepositoryHealthLastFullRepack? LastFullRepack { get; init; }

    /// <summary>When the statistics themselves were last computed.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }
}