using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for <c>GET /groups/:id/epics</c>.</summary>
[GitLabQuery]
public sealed record EpicListOptions
{
    public GitLabEpicOrderBy? OrderBy { get; init; }

    public GitLabEpicSort? Sort { get; init; }

    public string? Search { get; init; }

    public GitLabEpicState? State { get; init; }

    /// <summary>Mutually exclusive with <see cref="AuthorUsername" />.</summary>
    public long? AuthorId { get; init; }

    /// <summary>Mutually exclusive with <see cref="AuthorId" />.</summary>
    public string? AuthorUsername { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    public bool? WithLabelsDetails { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public bool? IncludeAncestorGroups { get; init; }

    public bool? IncludeDescendantGroups { get; init; }

    public string? MyReactionEmoji { get; init; }

    public bool? Confidential { get; init; }

    public int? PerPage { get; init; }

    [QueryParameter("not[labels]")] public IReadOnlyList<string>? NotLabels { get; init; }

    /// <summary>Mutually exclusive with <see cref="NotAuthorUsername" />.</summary>
    [QueryParameter("not[author_id]")]
    public long? NotAuthorId { get; init; }

    /// <summary>Mutually exclusive with <see cref="NotAuthorId" />.</summary>
    [QueryParameter("not[author_username]")]
    public string? NotAuthorUsername { get; init; }
}