using System.Text;

namespace GitLab.Client.Models;

/// <summary>
///     Google Cloud Storage credentials for an offline transfer, using S3-interoperability HMAC keys.
/// </summary>
public sealed record OfflineTransferGcsHmacConfiguration
{
    /// <summary>The GCS HMAC access key ID.</summary>
    public required string GoogleStorageAccessKeyId { get; init; }

    /// <summary>The GCS HMAC secret. Never logged.</summary>
    public required string GoogleStorageSecretAccessKey { get; init; }

    /// <summary>The bucket's region.</summary>
    public required string Region { get; init; }

    /// <summary>Addresses the bucket as <c>endpoint/bucket</c>. GitLab defaults this to true here.</summary>
    public bool? PathStyle { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("GoogleStorageAccessKeyId = ").Append(GoogleStorageAccessKeyId)
            .Append(", GoogleStorageSecretAccessKey = [redacted], Region = ").Append(Region)
            .Append(", PathStyle = ").Append(PathStyle);

        return true;
    }
}