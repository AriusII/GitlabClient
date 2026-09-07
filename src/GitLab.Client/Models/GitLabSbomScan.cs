namespace GitLab.Client.Models;

/// <summary>
///     An SBOM scan attached to a CI job (<c>/jobs/:id/sbom_scans</c>) - the result of scanning an
///     uploaded software bill of materials against GitLab's advisory database.
/// </summary>
public sealed record GitLabSbomScan
{
    public required long Id { get; init; }

    /// <summary>Where the finished scan report can be downloaded from.</summary>
    public Uri? DownloadUrl { get; init; }

    /// <summary>Whether the project has exceeded its scan rate limit and the request was rejected.</summary>
    public bool? Throttled { get; init; }

    /// <summary>Seconds until the project's throttling window resets, when <see cref="Throttled" /> is set.</summary>
    public int? ProjectThrottlingResetsIn { get; init; }

    /// <summary>The advisory database's freshness, as a bare string; GitLab types it as one.</summary>
    public string? AdvisoryDbState { get; init; }
}