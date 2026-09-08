using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for downloading an ml_model package file (<c>GET .../packages/ml_models/:model_version_id/files/...</c>).
/// </summary>
[GitLabQuery]
public sealed record MlModelPackageFileStatusOptions
{
    /// <summary>
    ///     Restricts the download to a file published with this status. GitLab defaults to serving
    ///     <see cref="GitLabPackageFileStatus.Default" /> files.
    /// </summary>
    public GitLabPackageFileStatus? Status { get; init; }
}