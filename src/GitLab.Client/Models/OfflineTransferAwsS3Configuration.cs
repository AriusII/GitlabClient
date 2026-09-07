using System.Text;

namespace GitLab.Client.Models;

/// <summary>AWS S3 credentials for an offline transfer export or import.</summary>
/// <remarks>
///     Carries a secret access key, so the record's synthesized formatting is replaced rather than
///     printing every property through <c>ToString()</c>.
/// </remarks>
public sealed record OfflineTransferAwsS3Configuration
{
    /// <summary>The AWS S3 access key ID.</summary>
    public required string AwsAccessKeyId { get; init; }

    /// <summary>The AWS S3 secret access key. Never logged.</summary>
    public required string AwsSecretAccessKey { get; init; }

    /// <summary>The bucket's region.</summary>
    public required string Region { get; init; }

    /// <summary>
    ///     Addresses the bucket as <c>s3.region.amazonaws.com/bucket</c> rather than
    ///     <c>bucket.s3.region.amazonaws.com</c>. GitLab defaults this to false for AWS.
    /// </summary>
    public bool? PathStyle { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("AwsAccessKeyId = ").Append(AwsAccessKeyId)
            .Append(", AwsSecretAccessKey = [redacted], Region = ").Append(Region)
            .Append(", PathStyle = ").Append(PathStyle);

        return true;
    }
}