using GitLab.Client.Models.Requests;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Options for <c>GET /jobs/:id/artifacts</c> - the project-agnostic, job-token-authenticated
///     artifact download that is part of the runner protocol (see <see cref="JobRequestRequest" />), as
///     opposed to <see cref="JobArtifactDownloadOptions" /> which scopes the same kind of download under
///     a project and an authenticated user.
/// </summary>
[GitLabQuery]
public readonly record struct JobArtifactsByTokenDownloadOptions
{
    /// <summary>The job's authentication token.</summary>
    public string? Token { get; init; }

    /// <summary>Downloads directly from remote storage instead of proxying the artifacts through GitLab.</summary>
    public bool? DirectDownload { get; init; }
}