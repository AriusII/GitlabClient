using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The version summary embedded in one <see cref="GitLabNugetSearchResult" />.</summary>
public sealed record GitLabNugetSearchResultVersion
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    public string? Version { get; init; }

    public int? Downloads { get; init; }
}