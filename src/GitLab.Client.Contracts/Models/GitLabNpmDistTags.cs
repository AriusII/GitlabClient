namespace GitLab.Client.Models;

/// <summary>The dist-tags of one npm package, keyed by tag name (for example <c>latest</c>) to version string.</summary>
public sealed record GitLabNpmDistTags
{
    /// <summary>Tag name to version string, e.g. <c>{ "latest": "1.0.1" }</c>.</summary>
    public IReadOnlyDictionary<string, string>? DistTags { get; init; }
}