using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /jobs/:id/sbom_scans/authorize</c>, which checks an SBOM upload against the
///     instance's size limit before the file itself is sent.
/// </summary>
public sealed record AuthorizeSbomScanUploadRequest
{
    /// <summary>
    ///     The size of the SBOM file, in bytes. GitLab spells the field <c>filesize</c>, as one word, which
    ///     the snake-case policy would otherwise render <c>file_size</c>.
    /// </summary>
    [JsonPropertyName("filesize")]
    public long? FileSize { get; init; }
}