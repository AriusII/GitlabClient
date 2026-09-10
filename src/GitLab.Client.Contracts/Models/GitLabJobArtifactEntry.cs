namespace GitLab.Client.Models;

/// <summary>
///     One entry in a job's artifacts archive, as returned by
///     <c>GET /projects/:id/jobs/:job_id/artifacts/tree</c>. Listing the archive is how a caller discovers
///     the <c>artifact_path</c> to hand to a single-file download.
/// </summary>
public sealed record GitLabJobArtifactEntry
{
    /// <summary>The entry's own name, without any directory component.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The full path of the entry inside the archive - <c>coverage/index.html</c>. This is the value a
    ///     single-file download takes as its <c>artifactPath</c>.
    /// </summary>
    public required string Path { get; init; }

    public GitLabJobArtifactEntryType? Type { get; init; }

    /// <summary>Uncompressed size in bytes. Zero for a directory.</summary>
    public long? Size { get; init; }

    /// <summary>The POSIX file mode the archive recorded, as an octal string - for example <c>100644</c>.</summary>
    public string? Mode { get; init; }
}