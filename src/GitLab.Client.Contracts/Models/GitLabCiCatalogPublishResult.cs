namespace GitLab.Client.Models;

/// <summary>The answer to <c>POST /projects/:id/catalog/publish</c>.</summary>
public sealed record GitLabCiCatalogPublishResult
{
    /// <summary>Where the newly published version can be browsed in the CI/CD catalog.</summary>
    public Uri? CatalogUrl { get; init; }
}