using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Pagination for <c>GET /namespaces/storage/limit_exclusions</c>.</summary>
[GitLabQuery]
public sealed record NamespaceStorageLimitExclusionListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}