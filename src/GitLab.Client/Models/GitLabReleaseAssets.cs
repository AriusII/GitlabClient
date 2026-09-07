namespace GitLab.Client.Models;

/// <summary>
///     The <c>assets</c> object GitLab nests under a release: the auto-generated source archives plus any
///     asset links attached to it.
/// </summary>
public sealed record GitLabReleaseAssets
{
    /// <summary>The total number of assets - <see cref="Sources" /> plus <see cref="Links" />.</summary>
    public int? Count { get; init; }

    public IReadOnlyList<GitLabReleaseSource>? Sources { get; init; }

    public IReadOnlyList<GitLabReleaseLink>? Links { get; init; }
}