namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>POST /offline_exports</c> - which bucket to write to, how to authenticate against
///     it, and what to export.
/// </summary>
/// <remarks>
///     Exactly one of the five configuration properties should be set; they are the mutually exclusive
///     ways of reaching <see cref="Bucket" />, and GitLab rejects the request when none is supplied.
/// </remarks>
public sealed record CreateOfflineExportRequest
{
    /// <summary>The object storage bucket the export is written to.</summary>
    public required string Bucket { get; init; }

    /// <summary>The groups and projects to export.</summary>
    public required IReadOnlyList<OfflineExportEntityRequest> Entities { get; init; }

    /// <summary>Authenticate against AWS S3 with an access key pair.</summary>
    public OfflineTransferAwsS3Configuration? AwsS3Configuration { get; init; }

    /// <summary>Authenticate against MinIO or another S3-compatible endpoint.</summary>
    public OfflineTransferS3CompatibleConfiguration? S3CompatibleConfiguration { get; init; }

    /// <summary>Authenticate against Google Cloud Storage with a service account JSON key.</summary>
    public OfflineTransferGcsConfiguration? GcsConfiguration { get; init; }

    /// <summary>Authenticate against Google Cloud Storage with S3-interoperability HMAC keys.</summary>
    public OfflineTransferGcsHmacConfiguration? GcsHmacConfiguration { get; init; }

    /// <summary>Authenticate against Google Cloud Storage with Application Default Credentials.</summary>
    public OfflineTransferGcsAdcConfiguration? GcsAdcConfiguration { get; init; }
}