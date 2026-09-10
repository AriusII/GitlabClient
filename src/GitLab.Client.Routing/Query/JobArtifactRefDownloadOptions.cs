using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Options for the two ref-scoped artifact downloads
///     (<c>GET /projects/:id/jobs/artifacts/:ref_name/download</c> and
///     <c>.../:ref_name/raw/:artifact_path</c>), which resolve a branch or tag to its latest successful job
///     rather than taking a job id.
/// </summary>
[GitLabQuery]
public readonly record struct JobArtifactRefDownloadOptions
{
    /// <summary>
    ///     A CI/CD job token, for reading another project's artifacts from inside a multi-project pipeline.
    ///     Premium and Ultimate only.
    /// </summary>
    public string? JobToken { get; init; }

    /// <summary>
    ///     Look through recent successful pipelines instead of only the most recent one. Useful when the
    ///     latest pipeline for the ref succeeded without running the job being asked for.
    /// </summary>
    public bool? SearchRecentSuccessfulPipelines { get; init; }
}