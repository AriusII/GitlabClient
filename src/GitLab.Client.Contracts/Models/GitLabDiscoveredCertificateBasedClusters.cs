using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The group and project partitions returned by
///     <c>GET /discover-cert-based-clusters</c>.
/// </summary>
/// <remarks>
///     GitLab 19.4 deliberately declares both partitions as untyped JSON objects. Keeping them as
///     <see cref="JsonElement" /> preserves all keys and values without falsely promising a stable map
///     shape that GitLab's OpenAPI schema does not define.
/// </remarks>
public sealed record GitLabDiscoveredCertificateBasedClusters
{
    public JsonElement? Groups { get; init; }

    public JsonElement? Projects { get; init; }
}