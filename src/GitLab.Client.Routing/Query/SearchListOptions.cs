using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters shared by the instance-wide (<c>GET /search</c>) and group-wide
///     (<c>GET /groups/:id/search</c>) search routes. The <c>scope</c> parameter is not here: it selects
///     the shape of the result, so each search method hard-codes its own and the caller never supplies it.
/// </summary>
[GitLabQuery]
public readonly record struct SearchListOptions
{
    /// <summary>"all", "opened", "closed" or "merged". Only meaningful for issue and merge-request searches.</summary>
    public string? State { get; init; }

    /// <summary>Filter by confidentiality. Only meaningful for issue searches.</summary>
    public bool? Confidential { get; init; }

    /// <summary>
    ///     Restricts work-item results by type ("issue", "task", "epic", "incident", "test_case",
    ///     "requirement", "objective", "key_result", "ticket"). Only applies to the <c>work_items</c> scope.
    ///     The spec types this as a bare string array rather than an enumerated one, so it stays
    ///     <see cref="string" /> here too - a type GitLab adds later must not turn a healthy response into a
    ///     <see cref="System.Text.Json.JsonException" />.
    /// </summary>
    public IReadOnlyList<string>? Type { get; init; }

    /// <summary>
    ///     Includes archived projects in instance-wide or group-wide searches. Introduced in GitLab 18.9;
    ///     the project-scoped route does not accept this parameter.
    /// </summary>
    public bool? IncludeArchived { get; init; }

    /// <summary>Fields to search, available with advanced search. Currently only "title" is accepted.</summary>
    public IReadOnlyList<string>? Fields { get; init; }

    /// <summary>Excludes forked projects. Available with exact code search; introduced in GitLab 18.9.</summary>
    public bool? ExcludeForks { get; init; }

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