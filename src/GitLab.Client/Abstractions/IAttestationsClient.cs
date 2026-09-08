using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Attestations" API area (<c>/projects/:id/attestations</c>) - build
///     provenance/artifact attestation bundles produced for a project's CI/CD pipelines, addressed by
///     their internal id (<c>iid</c>) rather than a global id.
/// </summary>
public interface IAttestationsClient
{
    /// <summary>
    ///     Downloads one attestation's raw bundle.
    /// </summary>
    /// <param name="projectId">The project the attestation belongs to.</param>
    /// <param name="attestationIid">The attestation's project-scoped internal id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     The open body plus its media type, length and file name. The caller owns it and must
    ///     <c>await using</c> it - it holds the HTTP response and its connection open until disposed.
    /// </returns>
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default);
}