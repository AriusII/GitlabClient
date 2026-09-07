using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The NuGet V3 Feed Service Index - the entry point of the protocol, listing the other services
///     (search, metadata, publish) and their base URLs. Returned by
///     <c>GET .../packages/nuget/index</c> at both project and group scope.
/// </summary>
public sealed record GitLabNugetServiceIndex
{
    public string? Version { get; init; }

    /// <summary>
    ///     Each entry is a small, loosely-typed object (<c>@id</c>, <c>@type</c>, <c>comment</c>) describing
    ///     one NuGet sub-service. The spec declares no fixed schema for it, so it is surfaced as a raw
    ///     <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    public IReadOnlyList<JsonElement>? Resources { get; init; }
}