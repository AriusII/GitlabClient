namespace GitLab.Client.Models;

/// <summary>
///     A project's GitLab Pages settings (<c>GET /projects/:id/pages</c>) - where the site is served, how
///     it is secured, and what is currently deployed.
/// </summary>
/// <remarks>
///     Every member is nullable on purpose. The pinned spec declares this response with no schema at all
///     (the Grape endpoint documents no entity), so the shape here is taken from GitLab's own Pages API
///     documentation. A field that turns out to be absent then deserializes to <see langword="null" />
///     rather than failing the whole call.
/// </remarks>
public sealed record GitLabPagesSettings
{
    /// <summary>The URL the project's Pages site is served from.</summary>
    public Uri? Url { get; init; }

    /// <summary>Whether the site is served from a unique, randomly suffixed domain.</summary>
    public bool? IsUniqueDomainEnabled { get; init; }

    /// <summary>Whether visitors are redirected from HTTP to HTTPS.</summary>
    public bool? ForceHttps { get; init; }

    /// <summary>The canonical domain the other domains redirect to, or <see langword="null" /> when none is set.</summary>
    public string? PrimaryDomain { get; init; }

    /// <summary>The currently published deployments - one per path prefix.</summary>
    public IReadOnlyList<GitLabPagesDeployment>? Deployments { get; init; }
}