using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing project topics (<c>GET /topics</c>).</summary>
[GitLabQuery]
public readonly record struct TopicListOptions
{
    /// <summary>Return only topics matching this search text.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Return only topics with no projects assigned. The orphans left behind after projects are retagged,
    ///     and the usual input to a cleanup pass.
    /// </summary>
    public bool? WithoutProjects { get; init; }

    /// <summary>Restrict the listing to one organization, on instances where organizations are enabled.</summary>
    public long? OrganizationId { get; init; }

    /// <summary>Page size. Pagination itself is followed automatically, so this only tunes the request count.</summary>
    public int? PerPage { get; init; }
}