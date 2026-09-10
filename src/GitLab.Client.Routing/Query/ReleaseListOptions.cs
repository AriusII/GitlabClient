using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a project's releases (<c>GET /projects/:id/releases</c>).</summary>
[GitLabQuery]
public readonly record struct ReleaseListOptions
{
    /// <summary>The field used for ordering: <c>released_at</c> (the default) or <c>created_at</c>.</summary>
    public string? OrderBy { get; init; }

    /// <summary>The sort direction: <c>asc</c> or <c>desc</c> (the default).</summary>
    public string? Sort { get; init; }

    /// <summary>Includes the Markdown-rendered description in each result.</summary>
    public bool? IncludeHtmlDescription { get; init; }

    /// <summary>Returns releases updated strictly before this UTC instant.</summary>
    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <summary>Returns releases updated strictly after this UTC instant.</summary>
    public DateTimeOffset? UpdatedAfter { get; init; }

    /// <summary>The number of releases requested in each page.</summary>
    public int? PerPage { get; init; }
}