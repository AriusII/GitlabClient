using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for the related-epic links visible within a group hierarchy.</summary>
[GitLabQuery]
public readonly record struct RelatedEpicLinkListOptions
{
    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public int? PerPage { get; init; }
}