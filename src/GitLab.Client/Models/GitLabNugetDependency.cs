using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>One dependency entry within a <see cref="GitLabNugetDependencyGroup" />.</summary>
public sealed record GitLabNugetDependency
{
    [JsonPropertyName("@id")] public Uri? AtId { get; init; }

    [JsonPropertyName("@type")] public string? AtType { get; init; }

    /// <summary>The dependency's NuGet package id.</summary>
    public string? Id { get; init; }

    /// <summary>The accepted version range, in NuGet range syntax.</summary>
    public string? Range { get; init; }
}