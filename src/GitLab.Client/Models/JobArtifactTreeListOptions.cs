using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing the files in a job's artifacts archive
///     (<c>GET /projects/:id/jobs/:job_id/artifacts/tree</c>).
/// </summary>
[GitLabQuery]
public sealed record JobArtifactTreeListOptions
{
    /// <summary>The directory inside the archive to browse. Defaults to the archive root.</summary>
    public string? Path { get; init; }

    /// <summary>When true, descend into every subdirectory instead of listing one level.</summary>
    public bool? Recursive { get; init; }

    /// <summary>
    ///     A CI/CD job token, for reading another project's artifacts from inside a multi-project pipeline.
    ///     Premium and Ultimate only.
    /// </summary>
    public string? JobToken { get; init; }

    public int? PerPage { get; init; }
}