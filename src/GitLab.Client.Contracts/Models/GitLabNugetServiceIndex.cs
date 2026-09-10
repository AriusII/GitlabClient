namespace GitLab.Client.Models;

/// <summary>
///     The NuGet V3 Feed Service Index - the entry point of the protocol, listing the other services
///     (search, metadata, publish) and their base URLs. Returned by
///     <c>GET .../packages/nuget/index</c> at both project and group scope.
/// </summary>
public sealed record GitLabNugetServiceIndex
{
    public string? Version { get; init; }

    /// <summary>Every endpoint advertised by the service index.</summary>
    public IReadOnlyList<GitLabNugetServiceResource>? Resources { get; init; }
}