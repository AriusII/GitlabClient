using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The NuGet Registration leaf - one package version's metadata, returned by
///     <c>GET .../packages/nuget/metadata/:package_name/:package_version</c>.
/// </summary>
/// <remarks>
///     Every multi-word member here carries an explicit <see cref="JsonPropertyNameAttribute" />: this is
///     the NuGet V3 protocol's own camelCase JSON convention, proxied by GitLab verbatim, not GitLab's
///     usual snake_case.
/// </remarks>
public sealed record GitLabNugetPackageMetadata
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    [JsonPropertyName("packageContent")] public Uri? PackageContent { get; init; }

    [JsonPropertyName("catalogEntry")] public GitLabNugetPackageMetadataCatalogEntry? CatalogEntry { get; init; }
}