using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One package hit within <see cref="GitLabNugetSearchResults" />. Every multi-word member here
///     carries an explicit <see cref="JsonPropertyNameAttribute" />: this is the NuGet V3 protocol's own
///     camelCase JSON convention, proxied by GitLab verbatim, not GitLab's usual snake_case.
/// </summary>
public sealed record GitLabNugetSearchResult
{
    [JsonPropertyName("@type")] public string? AtType { get; init; }

    /// <summary>The NuGet package id, e.g. <c>MyNuGetPkg</c>.</summary>
    public string? Id { get; init; }

    public string? Title { get; init; }

    [JsonPropertyName("totalDownloads")] public int? TotalDownloads { get; init; }

    public bool? Verified { get; init; }

    public string? Version { get; init; }

    /// <summary>
    ///     Named <c>versions</c> on the wire but carries a single entry, as GitLab's own OpenAPI document
    ///     declares it - not a list, despite the name.
    /// </summary>
    public GitLabNugetSearchResultVersion? Versions { get; init; }

    public string? Tags { get; init; }

    public string? Authors { get; init; }

    public string? Description { get; init; }

    public string? Summary { get; init; }

    [JsonPropertyName("projectUrl")] public Uri? ProjectUrl { get; init; }

    [JsonPropertyName("licenseUrl")] public Uri? LicenseUrl { get; init; }

    [JsonPropertyName("iconUrl")] public Uri? IconUrl { get; init; }
}