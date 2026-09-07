namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PATCH /projects/:id/pages</c>. Only the members that are set are sent; anything left
///     <see langword="null" /> keeps its current value.
/// </summary>
public sealed record UpdatePagesSettingsRequest
{
    /// <summary>
    ///     Whether the site is served from a unique, randomly suffixed domain instead of the predictable
    ///     one derived from the project path.
    /// </summary>
    public bool? PagesUniqueDomainEnabled { get; init; }

    /// <summary>Whether visitors are redirected from HTTP to HTTPS.</summary>
    public bool? PagesHttpsOnly { get; init; }

    /// <summary>
    ///     Which of the project's domains is canonical; the others redirect to it. Must be one of the
    ///     project's own Pages domains, or the unique domain.
    /// </summary>
    public string? PagesPrimaryDomain { get; init; }
}