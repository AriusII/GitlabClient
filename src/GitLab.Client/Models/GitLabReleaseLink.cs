namespace GitLab.Client.Models;

/// <summary>
///     One asset link attached to a release, as returned by
///     <c>/projects/:id/releases/:tag_name/assets/links</c> and nested in
///     <see cref="GitLabReleaseAssets.Links" />.
/// </summary>
public sealed record GitLabReleaseLink
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>The URL the link points at. Unique within the release.</summary>
    public Uri? Url { get; init; }

    /// <summary>
    ///     The permanent GitLab-hosted URL that redirects to <see cref="Url" />, present when the link was
    ///     created with a <c>direct_asset_path</c>.
    /// </summary>
    public Uri? DirectAssetUrl { get; init; }

    public GitLabReleaseLinkType? LinkType { get; init; }

    /// <summary>Whether <see cref="Url" /> points outside the GitLab instance hosting the release.</summary>
    public bool? External { get; init; }
}