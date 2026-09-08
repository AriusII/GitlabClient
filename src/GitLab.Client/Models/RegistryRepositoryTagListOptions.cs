using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging options for <c>GET /projects/:id/registry/repositories/:repository_id/tags</c>.</summary>
[GitLabQuery]
public readonly record struct RegistryRepositoryTagListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}