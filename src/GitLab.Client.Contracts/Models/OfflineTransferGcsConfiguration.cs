using System.Text;

namespace GitLab.Client.Models;

/// <summary>
///     Google Cloud Storage credentials for an offline transfer, using a service account JSON key.
/// </summary>
public sealed record OfflineTransferGcsConfiguration
{
    /// <summary>The Google Cloud project ID.</summary>
    public required string GoogleProject { get; init; }

    /// <summary>The service account JSON key, as its literal file contents. Never logged.</summary>
    public required string GoogleJsonKeyString { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("GoogleProject = ").Append(GoogleProject)
            .Append(", GoogleJsonKeyString = [redacted]");

        return true;
    }
}