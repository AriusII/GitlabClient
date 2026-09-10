using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     A project or group badge, as returned by the GitLab Badges API (<c>/projects/:id/badges</c>,
///     <c>/groups/:id/badges</c>).
/// </summary>
public sealed record GitLabBadge
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    /// <summary>
    ///     The unrendered link target, which may contain GitLab placeholders such as
    ///     <c>%{project_path}</c>. See <see cref="RenderedLinkUrl" /> for the resolved form.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "Unrendered badge URLs carry GitLab placeholders - %{project_path}, %{default_branch}, "
            + "%{commit_sha} - and '%{' is not a valid percent-escape, so System.Uri cannot parse them "
            + "reliably. Uri is used for the rendered_* members, which are always fully resolved.")]
    public required string LinkUrl { get; init; }

    /// <summary>The unrendered image source, which may contain the same GitLab placeholders as <see cref="LinkUrl" />.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See LinkUrl: placeholder-bearing badge URLs are not parseable as System.Uri.")]
    public required string ImageUrl { get; init; }

    /// <summary>The link target with every placeholder resolved for the project or group that owns the badge.</summary>
    public Uri? RenderedLinkUrl { get; init; }

    /// <summary>The image source with every placeholder resolved for the project or group that owns the badge.</summary>
    public Uri? RenderedImageUrl { get; init; }

    /// <summary>
    ///     "project" or "group". A project's badge list also returns the badges inherited from its groups, so
    ///     this is how a caller tells them apart.
    /// </summary>
    public string? Kind { get; init; }
}