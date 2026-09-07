using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's branches (<c>GET /projects/:id/repository/branches</c>).</summary>
[GitLabQuery]
public sealed record BranchListOptions
{
    /// <summary>Substring match on the branch name.</summary>
    public string? Search { get; init; }

    /// <summary>Regular expression the branch name must match.</summary>
    public string? Regex { get; init; }

    /// <summary>
    ///     "name_asc", "updated_asc" or "updated_desc". GitLab does not offer a "name_desc" here.
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>Branch name to start keyset pagination from.</summary>
    public string? PageToken { get; init; }

    public int? PerPage { get; init; }
}