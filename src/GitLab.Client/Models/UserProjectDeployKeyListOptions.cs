using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging options for <c>GET /users/:user_id/project_deploy_keys</c>.</summary>
[GitLabQuery]
public readonly record struct UserProjectDeployKeyListOptions
{
    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}