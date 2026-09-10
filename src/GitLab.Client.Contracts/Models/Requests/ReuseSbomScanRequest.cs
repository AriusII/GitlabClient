namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /jobs/:id/sbom_scans/:sbom_digest</c>, which reuses an existing scan instead
///     of uploading the same SBOM again.
/// </summary>
public sealed record ReuseSbomScanRequest
{
    /// <summary>
    ///     The package URL types the digest's scan must cover, such as <c>npm</c>, <c>maven</c> or
    ///     <c>golang</c>. GitLab declares the array's item type as untyped, so these stay strings.
    /// </summary>
    public required IReadOnlyList<string> PurlTypes { get; init; }
}