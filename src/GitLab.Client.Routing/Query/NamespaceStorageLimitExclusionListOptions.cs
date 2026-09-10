using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Pagination for <c>GET /namespaces/storage/limit_exclusions</c>.</summary>
[GitLabQuery]
public readonly record struct NamespaceStorageLimitExclusionListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}