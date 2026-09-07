using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for downloading a job's artifacts (<c>GET /projects/:id/jobs/:job_id/artifacts</c>).
/// </summary>
[GitLabQuery]
public sealed record JobArtifactDownloadOptions
{
    /// <summary>
    ///     Which artifact to download. Leave unset for the job's artifacts archive, which is what GitLab
    ///     defaults to.
    /// </summary>
    public GitLabJobArtifactFileType? FileType { get; init; }

    /// <summary>
    ///     A CI/CD job token, for reading another project's artifacts from inside a multi-project pipeline.
    ///     Premium and Ultimate only.
    /// </summary>
    public string? JobToken { get; init; }
}