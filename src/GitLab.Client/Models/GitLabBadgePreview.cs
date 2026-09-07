using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The result of previewing a badge before saving it (<c>GET /projects/:id/badges/render</c>,
///     <c>GET /groups/:id/badges/render</c>): the supplied URLs echoed back alongside the forms GitLab
///     would render them into.
///     <para>
///         Deliberately a separate type from <see cref="GitLabBadge" /> rather than a reuse: the render
///         response carries no <c>id</c> and no <c>kind</c>, so deserializing it into a shape with a
///         required id would throw.
///     </para>
/// </summary>
public sealed record GitLabBadgePreview
{
    public string? Name { get; init; }

    /// <summary>The unrendered link target exactly as it was submitted, placeholders included.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See GitLabBadge.LinkUrl: placeholder-bearing badge URLs are not parseable as System.Uri.")]
    public required string LinkUrl { get; init; }

    /// <summary>The unrendered image source exactly as it was submitted, placeholders included.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See GitLabBadge.LinkUrl: placeholder-bearing badge URLs are not parseable as System.Uri.")]
    public required string ImageUrl { get; init; }

    /// <summary>The link target with every placeholder resolved.</summary>
    public Uri? RenderedLinkUrl { get; init; }

    /// <summary>The image source with every placeholder resolved.</summary>
    public Uri? RenderedImageUrl { get; init; }
}