namespace GitLab.Client.Models;

/// <summary>
///     A provenance attestation recorded for a build artifact
///     (<c>GET /projects/:id/attestations/:subject_digest</c>) - the SLSA statement tying an artifact
///     hash back to the job that produced it.
/// </summary>
/// <remarks>
///     GitLab gates these endpoints behind a feature flag and describes them as not ready for production
///     use, so an instance may answer <c>404</c> even for a project the caller can otherwise read.
/// </remarks>
public sealed record GitLabAttestation
{
    public required long Id { get; init; }

    /// <summary>The per-project sequence number, which is what the download route takes.</summary>
    public long? Iid { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>When the stored attestation bundle is scheduled to be deleted.</summary>
    public DateTimeOffset? ExpireAt { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The CI job that produced the attested artifact.</summary>
    public long? BuildId { get; init; }

    /// <summary>The attestation's processing state. Kept as a bare string; GitLab types it as one.</summary>
    public string? Status { get; init; }

    /// <summary>The kind of predicate the statement carries, such as <c>provenance</c>.</summary>
    public string? PredicateKind { get; init; }

    /// <summary>The predicate type URI, such as <c>https://slsa.dev/provenance/v1</c>.</summary>
    public string? PredicateType { get; init; }

    /// <summary>The SHA-256 hash of the artifact the statement is about.</summary>
    public string? SubjectDigest { get; init; }

    /// <summary>Where the attestation bundle can be downloaded from.</summary>
    public Uri? DownloadUrl { get; init; }
}