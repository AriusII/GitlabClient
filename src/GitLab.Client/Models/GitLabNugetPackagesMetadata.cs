namespace GitLab.Client.Models;

/// <summary>
///     The NuGet Registration index for one package name - every version's metadata, returned by
///     <c>GET .../packages/nuget/metadata/:package_name/index</c>.
/// </summary>
public sealed record GitLabNugetPackagesMetadata
{
    public int Count { get; init; }

    public IReadOnlyList<GitLabNugetPackagesMetadataItem>? Items { get; init; }
}