using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a group's labels (<c>GET /groups/:id/labels</c>).</summary>
[GitLabQuery]
public readonly record struct GroupLabelListOptions
{
    /// <summary>Fill in the issue and merge request counts, which GitLab omits by default.</summary>
    public bool? WithCounts { get; init; }

    /// <summary>Include labels inherited from ancestor groups.</summary>
    public bool? IncludeAncestorGroups { get; init; }

    /// <summary>Include labels defined on descendant groups.</summary>
    public bool? IncludeDescendantGroups { get; init; }

    /// <summary>Return only group labels, leaving out the labels of the group's projects.</summary>
    public bool? OnlyGroupLabels { get; init; }

    /// <summary>Keyword matched against the label name and description.</summary>
    public string? Search { get; init; }

    /// <summary>Filter by archived status.</summary>
    public bool? Archived { get; init; }

    public int? PerPage { get; init; }
}