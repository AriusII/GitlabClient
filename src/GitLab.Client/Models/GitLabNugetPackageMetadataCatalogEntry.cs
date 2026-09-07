using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The NuGet catalog entry embedded in <see cref="GitLabNugetPackageMetadata" /> - the package's own
///     descriptive metadata (id, version, authors, dependency groups) as opposed to the envelope around it.
/// </summary>
/// <remarks>
///     Every multi-word member here carries an explicit <see cref="JsonPropertyNameAttribute" />: this is
///     the NuGet V3 protocol's own camelCase JSON convention, proxied by GitLab verbatim, not GitLab's
///     usual snake_case.
/// </remarks>
public sealed record GitLabNugetPackageMetadataCatalogEntry
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    [JsonPropertyName("dependencyGroups")]
    public IReadOnlyList<GitLabNugetDependencyGroup>? DependencyGroups { get; init; }

    /// <summary>The NuGet package id, e.g. <c>MyNuGetPkg</c>.</summary>
    public string? Id { get; init; }

    public string? Version { get; init; }

    /// <summary>Space-separated tags, as NuGet's <c>.nuspec</c> stores them.</summary>
    public string? Tags { get; init; }

    [JsonPropertyName("packageContent")] public Uri? PackageContent { get; init; }

    public string? Authors { get; init; }

    public string? Description { get; init; }

    public string? Summary { get; init; }

    [JsonPropertyName("projectUrl")] public Uri? ProjectUrl { get; init; }

    [JsonPropertyName("licenseUrl")] public Uri? LicenseUrl { get; init; }

    [JsonPropertyName("iconUrl")] public Uri? IconUrl { get; init; }

    public DateTimeOffset? Published { get; init; }
}