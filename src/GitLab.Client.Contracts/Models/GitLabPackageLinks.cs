namespace GitLab.Client.Models;

/// <summary>The <c>_links</c> object on a <see cref="GitLabPackage" />.</summary>
public sealed record GitLabPackageLinks
{
    /// <summary>The web UI route for this package, relative to the instance root.</summary>
    public string? WebPath { get; init; }

    /// <summary>The API route that deletes this package, relative to the instance root.</summary>
    public string? DeleteApiPath { get; init; }
}