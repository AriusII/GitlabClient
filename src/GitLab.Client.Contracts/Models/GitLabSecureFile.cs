using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A CI secure file (<c>/projects/:id/secure_files</c>) - a certificate, provisioning profile or
///     keystore stored outside the repository and pulled into a job by the <c>download-secure-files</c>
///     tool.
///     <para>
///         The file's contents are never part of this shape: they come only from
///         <c>GET /projects/:id/secure_files/:id/download</c>, which returns a raw binary body.
///     </para>
/// </summary>
public sealed record GitLabSecureFile
{
    public required long Id { get; init; }

    /// <summary>The file name, unique within the project.</summary>
    public required string Name { get; init; }

    /// <summary>Digest of the stored contents, computed with <see cref="ChecksumAlgorithm" />.</summary>
    public string? Checksum { get; init; }

    /// <summary>The digest algorithm - <c>sha256</c> for every file GitLab stores today.</summary>
    public string? ChecksumAlgorithm { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     When the certificate inside the file expires, if GitLab could parse one out of it. Null for file
    ///     types that carry no expiry.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     Whatever GitLab parsed out of the file - issuer and subject for a <c>.cer</c>, app id and
    ///     devices for a <c>.mobileprovision</c>. The spec types it as an untyped object and its members
    ///     differ per file type, so it is captured as a raw <see cref="JsonElement" /> rather than being
    ///     forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Metadata { get; init; }

    /// <summary>The extension GitLab derived from <see cref="Name" /> - <c>cer</c>, <c>p12</c>, and so on.</summary>
    public string? FileExtension { get; init; }
}