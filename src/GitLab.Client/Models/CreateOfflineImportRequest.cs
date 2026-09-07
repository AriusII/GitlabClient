namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>POST /offline_imports</c> - which bucket to read a previously written offline
///     transfer export back out of, and what to import from it.
/// </summary>
/// <remarks>
///     Exactly one of the five configuration properties should be set; they are the mutually exclusive
///     ways of reaching <see cref="Bucket" />, and GitLab rejects the request when none is supplied.
/// </remarks>
public sealed record CreateOfflineImportRequest
{
    /// <summary>The object storage bucket the export was written to.</summary>
    public required string Bucket { get; init; }

    /// <summary>The groups and projects to import out of the export.</summary>
    public required IReadOnlyList<OfflineImportEntityRequest> Entities { get; init; }

    /// <summary>The prefix the export was written under inside <see cref="Bucket" />.</summary>
    public string? ExportPrefix { get; init; }

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