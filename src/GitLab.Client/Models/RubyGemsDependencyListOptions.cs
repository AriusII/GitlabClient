using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for retrieving RubyGems dependencies
///     (<c>GET /projects/:id/packages/rubygems/api/v1/dependencies</c>).
/// </summary>
[GitLabQuery]
public sealed record RubyGemsDependencyListOptions
{
    /// <summary>Gem names to retrieve dependencies for. Omit to retrieve every gem in the registry.</summary>
    public IReadOnlyList<string>? Gems { get; init; }
}