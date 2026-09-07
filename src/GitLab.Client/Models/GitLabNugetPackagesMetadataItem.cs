using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>One registration page within <see cref="GitLabNugetPackagesMetadata" />.</summary>
public sealed record GitLabNugetPackagesMetadataItem
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    public string? Lower { get; init; }

    public string? Upper { get; init; }

    public int? Count { get; init; }

    public IReadOnlyList<GitLabNugetPackageMetadata>? Items { get; init; }
}