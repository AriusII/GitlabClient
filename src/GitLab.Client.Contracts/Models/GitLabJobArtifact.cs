namespace GitLab.Client.Models;

/// <summary>
///     One artifact produced by a CI/CD job, as nested under <see cref="GitLabJob.Artifacts" />.
/// </summary>
public sealed record GitLabJobArtifact
{
    /// <summary>
    ///     What the artifact is - "archive", "metadata", "trace", "junit", "sast", "cobertura", "jacoco",
    ///     "dotenv" and about two dozen more. Kept a plain string rather than an enum, per this library's rule
    ///     for GitLab's open-ended status and kind fields: GitLab adds values here every few releases, and an
    ///     enum would turn a new one into a deserialization failure.
    /// </summary>
    public string? FileType { get; init; }

    /// <summary>Size in bytes.</summary>
    public long? Size { get; init; }

    public string? Filename { get; init; }

    /// <summary>How the artifact is packaged - "raw", "zip" or "gzip".</summary>
    public string? FileFormat { get; init; }
}