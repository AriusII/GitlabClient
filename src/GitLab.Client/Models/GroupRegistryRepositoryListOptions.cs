using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Paging options for <c>GET /groups/:id/registry/repositories</c>. Unlike the project-scoped
///     listing, GitLab does not declare <c>tags</c>/<c>tags_count</c> inclusion flags on this route.
/// </summary>
[GitLabQuery]
public readonly record struct GroupRegistryRepositoryListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}