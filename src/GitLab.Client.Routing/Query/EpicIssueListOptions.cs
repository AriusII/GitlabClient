using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Paging control for issues listed under one epic.</summary>
[GitLabQuery]
public readonly record struct EpicIssueListOptions
{
    /// <summary>Items per response page. GitLab defaults to 20.</summary>
    public int? PerPage { get; init; }
}