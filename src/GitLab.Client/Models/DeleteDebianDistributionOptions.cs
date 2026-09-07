using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Optional match filters on <c>DELETE /projects/:id/debian_distributions/:codename</c> and
///     <c>DELETE /groups/:id/-/debian_distributions/:codename</c>, alongside the required <c>codename</c>
///     path segment. The spec declares the same attribute set here as on the list and update endpoints,
///     <c>version</c> included - which the update body itself does not accept.
/// </summary>
[GitLabQuery]
public sealed record DeleteDebianDistributionOptions
{
    public string? Suite { get; init; }

    public string? Origin { get; init; }

    public string? Label { get; init; }

    public string? Version { get; init; }

    public string? Description { get; init; }

    public int? ValidTimeDurationSeconds { get; init; }

    public IReadOnlyList<string>? Components { get; init; }

    public IReadOnlyList<string>? Architectures { get; init; }
}