using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a project's labels (<c>GET /projects/:id/labels</c>).</summary>
[GitLabQuery]
public readonly record struct LabelListOptions
{
    public string? Search { get; init; }

    /// <summary>Fill in the issue and merge request counts, which GitLab omits by default.</summary>
    public bool? WithCounts { get; init; }

    /// <summary>Include the labels the project inherits from its ancestor groups.</summary>
    public bool? IncludeAncestorGroups { get; init; }

    /// <summary>Filter by archived status.</summary>
    public bool? Archived { get; init; }

    public int? PerPage { get; init; }
}