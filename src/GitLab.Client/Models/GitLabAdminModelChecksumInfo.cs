namespace GitLab.Client.Models;

/// <summary>
///     The checksum state nested inside <see cref="GitLabAdminModelRecord.ChecksumInformation" />. Every
///     member is exactly as GitLab types it in the OpenAPI document, including
///     <see cref="ChecksumRetryCount" /> being a string rather than a number.
/// </summary>
public sealed record GitLabAdminModelChecksumInfo
{
    public string? Checksum { get; init; }

    public string? LastChecksum { get; init; }

    public string? ChecksumState { get; init; }

    public string? ChecksumRetryCount { get; init; }

    public string? ChecksumRetryAt { get; init; }

    public string? ChecksumFailure { get; init; }
}