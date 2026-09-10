namespace GitLab.Client.Models;

/// <summary>
///     One auto-generated source archive of a release's tag, nested in
///     <see cref="GitLabReleaseAssets.Sources" />.
/// </summary>
public sealed record GitLabReleaseSource
{
    /// <summary>The archive format - <c>zip</c>, <c>tar.gz</c>, <c>tar.bz2</c> or <c>tar</c>.</summary>
    public string? Format { get; init; }

    public Uri? Url { get; init; }
}