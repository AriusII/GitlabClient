using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/badges/:badge_id</c> and
///     <c>PUT /groups/:id/badges/:badge_id</c>. Every member is optional and an unset one is omitted from
///     the payload rather than sent as null, so a partial update cannot blank a field it did not mean to
///     touch.
/// </summary>
public sealed record UpdateBadgeRequest
{
    /// <summary>Where the badge links to. May contain GitLab placeholders such as <c>%{project_path}</c>.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See CreateBadgeRequest.LinkUrl: badge URLs must be able to carry unescaped placeholders.")]
    public string? LinkUrl { get; init; }

    /// <summary>The badge image source. May contain GitLab placeholders such as <c>%{project_path}</c>.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See CreateBadgeRequest.LinkUrl: badge URLs must be able to carry unescaped placeholders.")]
    public string? ImageUrl { get; init; }

    public string? Name { get; init; }
}