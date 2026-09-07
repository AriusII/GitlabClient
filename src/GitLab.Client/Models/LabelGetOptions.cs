using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Options for retrieving a single project label (<c>GET /projects/:id/labels/:name</c>).</summary>
[GitLabQuery]
public sealed record LabelGetOptions
{
    /// <summary>Also match labels inherited from the project's ancestor groups.</summary>
    public bool? IncludeAncestorGroups { get; init; }
}