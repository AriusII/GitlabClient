using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the NuGet Search Service (<c>GET .../packages/nuget/query</c>), at both project and
///     group scope.
/// </summary>
[GitLabQuery]
public readonly record struct NugetSearchOptions
{
    /// <summary>The search term. Left unset, GitLab returns every package.</summary>
    public string? Q { get; init; }

    public int? Skip { get; init; }

    public int? Take { get; init; }

    /// <summary>Whether to include prerelease versions. GitLab defaults this to false.</summary>
    public bool? Prerelease { get; init; }
}