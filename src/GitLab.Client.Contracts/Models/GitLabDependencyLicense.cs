namespace GitLab.Client.Models;

/// <summary>
///     One licence a dependency is distributed under, as embedded in
///     <see cref="GitLabDependency.Licenses" />.
/// </summary>
public sealed record GitLabDependencyLicense
{
    /// <summary>The SPDX short identifier, such as <c>MIT</c> or <c>Apache-2.0</c>.</summary>
    public string? SpdxIdentifier { get; init; }

    /// <summary>The human-readable licence name.</summary>
    public string? Name { get; init; }

    /// <summary>Where the licence text lives.</summary>
    public Uri? Url { get; init; }
}