using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The NuGet Search Service response, returned by <c>GET .../packages/nuget/query</c> at both project
///     and group scope.
/// </summary>
public sealed record GitLabNugetSearchResults
{
    [JsonPropertyName("totalHits")] public int TotalHits { get; init; }

    public IReadOnlyList<GitLabNugetSearchResult>? Data { get; init; }
}