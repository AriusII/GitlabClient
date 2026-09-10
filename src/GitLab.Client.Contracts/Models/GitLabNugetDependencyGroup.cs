using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One target-framework's dependency set within a NuGet package's catalog entry. Field names follow
///     the NuGet V3 protocol's own camelCase JSON convention, proxied by GitLab verbatim.
/// </summary>
public sealed record GitLabNugetDependencyGroup
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    [JsonPropertyName("@type")] public string? AtType { get; init; }

    [JsonPropertyName("targetFramework")] public string? TargetFramework { get; init; }

    public IReadOnlyList<GitLabNugetDependency>? Dependencies { get; init; }
}