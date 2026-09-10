using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Options for retrieving a single group label (<c>GET /groups/:id/labels/:name</c>).</summary>
[GitLabQuery]
public readonly record struct GroupLabelGetOptions
{
    /// <summary>Also search ancestor groups for the label.</summary>
    public bool? IncludeAncestorGroups { get; init; }

    /// <summary>Also search descendant groups for the label.</summary>
    public bool? IncludeDescendantGroups { get; init; }

    /// <summary>Only consider group labels, not the labels of the group's projects.</summary>
    public bool? OnlyGroupLabels { get; init; }
}