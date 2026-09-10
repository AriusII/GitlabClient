namespace GitLab.Client.Models;

/// <summary>The hypermedia links GitLab exposes in a project's <c>_links</c> member.</summary>
public sealed record GitLabProjectLinks
{
    public Uri? Self { get; init; }

    public Uri? Issues { get; init; }

    public Uri? MergeRequests { get; init; }

    public Uri? RepoBranches { get; init; }

    public Uri? Labels { get; init; }

    public Uri? Events { get; init; }

    public Uri? Members { get; init; }

    public Uri? ClusterAgents { get; init; }
}