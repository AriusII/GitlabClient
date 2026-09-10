using System.Text;

namespace GitLab.Client.Models;

/// <summary>
///     MinIO or other S3-compatible object storage credentials for an offline transfer export or import.
///     Distinct from <see cref="OfflineTransferAwsS3Configuration" /> only by the required
///     <see cref="Endpoint" />, which is what tells GitLab not to talk to AWS.
/// </summary>
public sealed record OfflineTransferS3CompatibleConfiguration
{
    /// <summary>The access key ID.</summary>
    public required string AwsAccessKeyId { get; init; }

    /// <summary>The secret access key. Never logged.</summary>
    public required string AwsSecretAccessKey { get; init; }

    /// <summary>The bucket's region.</summary>
    public required string Region { get; init; }

    /// <summary>The object storage endpoint, for example <c>https://minio.example.com</c>.</summary>
    public required Uri Endpoint { get; init; }

    /// <summary>
    ///     Addresses the bucket as <c>endpoint/bucket</c> rather than as a virtual host. GitLab defaults
    ///     this to true for S3-compatible storage, which is what most MinIO deployments need.
    /// </summary>
    public bool? PathStyle { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("AwsAccessKeyId = ").Append(AwsAccessKeyId)
            .Append(", AwsSecretAccessKey = [redacted], Region = ").Append(Region)
            .Append(", Endpoint = ").Append(Endpoint)
            .Append(", PathStyle = ").Append(PathStyle);

        return true;
    }
}