using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the project-scoped search route (<c>GET /projects/:id/search</c>). Separate from
///     <see cref="SearchListOptions" /> because <see cref="Ref" /> is accepted only here - folding it into
///     the shared type would advertise a parameter the instance and group routes silently ignore.
/// </summary>
[GitLabQuery]
public readonly record struct ProjectSearchListOptions
{
    /// <summary>Branch or tag to search. Defaults to the project's default branch.</summary>
    public string? Ref { get; init; }

    /// <summary>"all", "opened", "closed" or "merged". Only meaningful for issue and merge-request searches.</summary>
    public string? State { get; init; }

    /// <summary>Filter by confidentiality. Only meaningful for issue searches.</summary>
    public bool? Confidential { get; init; }

    /// <summary>
    ///     Restricts work-item results by type ("issue", "task", "epic", "incident", "test_case",
    ///     "requirement", "objective", "key_result", "ticket"). Only applies to the <c>work_items</c> scope.
    ///     Kept as <see cref="string" /> rather than an enum - see <see cref="SearchListOptions.Type" />.
    /// </summary>
    public IReadOnlyList<string>? Type { get; init; }

    /// <summary>Fields to search, available with advanced search. Currently only "title" is accepted.</summary>
    public IReadOnlyList<string>? Fields { get; init; }

    /// <summary>
    ///     Number of context lines around each match (0-20). Available with advanced and exact code search;
    ///     introduced in GitLab 18.11.
    /// </summary>
    public int? NumContextLines { get; init; }

    /// <summary>Performs a regex code search. Available with exact code search; introduced in GitLab 18.9.</summary>
    public bool? Regex { get; init; }

    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}