using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging options for <c>GET /projects/:id/registry/repositories/:repository_id/tags</c>.</summary>
[GitLabQuery]
public sealed record RegistryRepositoryTagListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}