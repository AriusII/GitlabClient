using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Pagination for <c>GET /internal/gitlab_subscriptions/namespaces/:id/projects</c>.</summary>
[GitLabQuery]
public sealed record NamespaceProjectListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}